using SelfDriveInstaller.Models;
using System.Diagnostics;
using System.Text;

namespace SelfDriveInstaller.Services;

internal class ScriptExecutor
{
    private readonly string _scriptDirectory;

    internal ScriptExecutor(string scriptDirectory)
    {
        _scriptDirectory = scriptDirectory;
    }

    internal string GetRunInstallScriptCommandForDebugging(string configPath)
    {
        var scriptPath = Path.Combine(_scriptDirectory, "Install-CheapSelfDrive.ps1");
        var arguments = BuildInstallArguments(configPath);
        var modifiedArguments = $"powershell.exe -ExecutionPolicy Bypass -NoProfile -NonInteractive -File \"{scriptPath}\" {arguments}";
        return modifiedArguments;
    }

    internal async Task<ExecutionResult> RunInstallScript(string configPathd)
    {
        try
        {
            var scriptPath = Path.Combine(_scriptDirectory, "Install-CheapSelfDrive.ps1");
            if (!File.Exists(scriptPath))
            {
                return new ExecutionResult
                {
                    Success = false,
                    Error = $"Install script not found at: {scriptPath}"
                };
            }

            var arguments = BuildInstallArguments(configPathd);
            return await ExecutePowerShellScript(scriptPath, arguments);
        }
        catch (Exception ex)
        {
            return new ExecutionResult
            {
                Success = false,
                Error = $"Failed to run install script: {ex.Message}"
            };
        }
    }

    internal async Task<ExecutionResult> RunMount(Config config)
    {
        try
        {
            var scriptPath = Path.Combine(config.InstallDirectory.Replace("{MountName}", config.MountName), $"Mount-{config.MountName}-SFTPDisk.ps1");
            if (!File.Exists(scriptPath))
            {
                return new ExecutionResult
                {
                    Success = false,
                    Error = $"Mount script not found at: {scriptPath}"
                };
            }
            
            // Execute as normal user (not elevated) so the network drive is accessible
            // to user applications. Use Windows Task Scheduler to run as interactive user.
            var taskName = $"CheapSelfDrive_Mount_{Guid.NewGuid():N}";
            var psCommand = $@"
$taskName = '{taskName}'
$action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument '-ExecutionPolicy Bypass -NoProfile -NonInteractive -File ""{scriptPath}""'
$principal = New-ScheduledTaskPrincipal -UserId $env:USERNAME -LogonType Interactive -RunLevel Limited
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable

Register-ScheduledTask -TaskName $taskName -Action $action -Principal $principal -Settings $settings -Force | Out-Null
Start-ScheduledTask -TaskName $taskName

# Wait for task to complete
$timeout = 300
$elapsed = 0
while ($elapsed -lt $timeout) {{
    $task = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
    if ($null -eq $task) {{ break }}
    
    $taskInfo = Get-ScheduledTaskInfo -TaskName $taskName -ErrorAction SilentlyContinue
    if ($task.State -eq 'Ready' -and $null -ne $taskInfo.LastRunTime) {{
        $exitCode = $taskInfo.LastTaskResult
        Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue
        exit $exitCode
    }}
    
    Start-Sleep -Seconds 1
    $elapsed++
}}

# Cleanup on timeout
Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue
Write-Error 'Mount script timed out'
exit 1
";
            
            return await ExecutePowerShellCommand(psCommand, 320);
        }
        catch (Exception ex)
        {
            return new ExecutionResult
            {
                Success = false,
                Error = $"Failed to run mount script: {ex.Message}"
            };
        }
    }

    internal async Task<ExecutionResult> RunUninstallScript(string mountName)
    {
        try
        {
            var scriptPath = Path.Combine(_scriptDirectory, "Uninstall-CheapSelfDrive.ps1");
            if (!File.Exists(scriptPath))
            {
                return new ExecutionResult
                {
                    Success = false,
                    Error = $"Uninstall script not found at: {scriptPath}"
                };
            }

            var arguments = $"-MountName '{mountName}'";
            return await ExecutePowerShellScript(scriptPath, arguments);
        }
        catch (Exception ex)
        {
            return new ExecutionResult
            {
                Success = false,
                Error = $"Failed to run uninstall script: {ex.Message}"
            };
        }
    }

    internal async Task<ExecutionResult> TestConnection(Config config, string password)
    {
        try
        {
            // Test SFTP connection using PowerShell
            var testScript = $@"
try {{
    $securePassword = ConvertTo-SecureString '{password}' -AsPlainText -Force
    $credential = New-Object System.Management.Automation.PSCredential('{config.NASUsername}', $securePassword)
    
    # Simple connection test using .NET SSH library or PowerShell SSH module
    # For now, we'll use a basic network test
    Test-NetConnection -ComputerName '{config.NASAddress}' -Port {config.NASPort} -InformationLevel Quiet
    if ($?) {{
        Write-Output 'Connection successful'
        exit 0
    }} else {{
        Write-Error 'Connection failed'
        exit 1
    }}
}} catch {{
    Write-Error $_.Exception.Message
    exit 1
}}";

            return await ExecutePowerShellCommand(testScript);
        }
        catch (Exception ex)
        {
            return new ExecutionResult
            {
                Success = false,
                Error = $"Failed to test connection: {ex.Message}"
            };
        }
    }

    internal void CopyFilesToInstallationDirectory(Config config)
    {
        var sourceDir = AppContext.BaseDirectory;
        var targetDir = Path.Combine(config.InstallDirectory, "bin");

        if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

        // Copy all the directory and subdirectory files
        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDir, file);
            var targetPath = Path.Combine(targetDir, relativePath);

            var targetSubDir = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(targetSubDir))
            {
                Directory.CreateDirectory(targetSubDir);
            }

            File.Copy(file, targetPath, true);
        }
    }

    internal string InstanciatingTemplateScriptMountDrive(Config config)
    {
        var templatePath = Path.Combine(config.InstallDirectory, "bin", "PowerShell", "Mount-TEMPLATE-SFTPDisk.ps1");
        var outputPath = Path.Combine(config.InstallDirectory, "Mount-" + config.MountName + "-SFTPDisk.ps1");

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template script not found at: {templatePath}");
        }

        var finalVFSCacheDir = config.VFSCacheDir.Replace(@"{MountName}", config.MountName);
        var finalLogsDir = config.RcloneLogs.Replace(@"{MountName}", config.MountName);
        var finalConfig = config.RcloneConfigDir.Replace(@"{APPDATA}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        var templateContent = File.ReadAllText(templatePath);
        var populatedContent = templateContent
            .Replace("{{MOUNT_NAME}}", config.MountName)
            .Replace("{{DRIVE_LETTER}}", config.DriveLetter)
            .Replace("{{VFS_CACHE_DIR}}", finalVFSCacheDir.Replace(@"\", @"\\"))
            .Replace("{{RCLONE_LOGS}}", finalLogsDir.Replace(@"\", @"\\"))
            .Replace("{{RCLONE_CONFIG}}", finalConfig.Replace(@"\", @"\\"));

        File.WriteAllText(outputPath, populatedContent);

        return outputPath;
    }

    internal async Task StartDrive(Config currentConfig)
    {
        var scriptPath = Path.Combine(_scriptDirectory, "Start-CheapSelfDrive.ps1");
        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException($"Start script not found at: {scriptPath}");
        }

        var arguments = $"-MountName '{currentConfig.MountName}' -DriveLetter '{currentConfig.DriveLetter}'";
        var result = await ExecutePowerShellScript(scriptPath, arguments);

        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to start drive: {result.Error}");
        }
    }

    internal MountStatus GetMountStatus(Config config)
    {
        try
        {
            // Check if drive is mounted
            var driveExists = CheckDriveExists(config.MountName, config.DriveLetter);
            var folderExists = Directory.Exists(config.GetExpandedVFSCacheDir());

            if (driveExists)
                return MountStatus.Running;
            else if (folderExists)
                return MountStatus.Installed;
            else
                return MountStatus.NotConfigured;
        }
        catch
        {
            return MountStatus.Error;
        }
    }

    internal async Task<ExecutionResult> LaunchSystrayApp(Config config)
    {
        try
        {
            var appPath = Path.Combine(config.InstallDirectory, "bin", "SelfDrive.Systray.exe");
            if (!File.Exists(appPath))
            {
                return new ExecutionResult
                {
                    Success = false,
                    Error = $"Systray app not found at: {appPath}"
                };
            }
            
            // Launch as normal user (not elevated) using Task Scheduler
            // This ensures the systray app runs in user context and can access user resources
            var taskName = $"CheapSelfDrive_Launch_{Guid.NewGuid():N}";
            var psCommand = $@"
$taskName = '{taskName}'
$action = New-ScheduledTaskAction -Execute '{appPath}'
$principal = New-ScheduledTaskPrincipal -UserId $env:USERNAME -LogonType Interactive -RunLevel Limited
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable

Register-ScheduledTask -TaskName $taskName -Action $action -Principal $principal -Settings $settings -Force | Out-Null
Start-ScheduledTask -TaskName $taskName

# Wait a moment for the app to start, then cleanup the task
Start-Sleep -Seconds 2
Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue
exit 0
";
            
            return await ExecutePowerShellCommand(psCommand, 10);
        }
        catch (Exception ex)
        {
            return new ExecutionResult
            {
                Success = false,
                Error = $"Failed to launch systray app: {ex.Message}"
            };
        }
    }

    internal void AddSystrayAppToStartup(Config config)
    {
        var appPath = Path.Combine(config.InstallDirectory, "bin", "SelfDrive.Systray.exe");
        
        // Use Registry Run key instead of shortcut - simpler and no COM dependencies
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
        
        if (key != null)
        {
            key.SetValue("SelfDrive.Systray", $"\"{appPath}\"");
        }
    }

    internal async Task ConfigureSystrayApp(Config currentConfig)
    {
        var driveMappingsPath = Path.Combine(currentConfig.InstallDirectory, "bin", "drive-mappings.txt");
        var scriptPath = Path.Combine(currentConfig.InstallDirectory, $"Mount-{currentConfig.MountName}-SFTPDisk.ps1");
        var driveLetter = currentConfig.DriveLetter;
        
        // Ensure drive letter ends with backslash for proper format
        if (!driveLetter.EndsWith(@"\"))
        {
            driveLetter += @"\";
        }
        
        var newEntry = $"{driveLetter}|{scriptPath}";
        
        // Read existing content or create new with header
        List<string> lines;
        if (File.Exists(driveMappingsPath))
        {
            lines = File.ReadAllLines(driveMappingsPath).ToList();
        }
        else
        {
            // Create directory if needed
            var dir = Path.GetDirectoryName(driveMappingsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            
            lines = new List<string>
            {
                "# Drive mappings: one per line",
                "# Format: <DriveRoot>|<PowerShellScriptPath>",
                $"# Example: F:\\|C:\\VFS\\Mount-FastNas-SFTPDisk.ps1",
                ""
            };
        }
        
        // Find and update or add the entry
        var entryFound = false;
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                continue;
                
            var parts = line.Split('|');
            if (parts.Length == 2 && parts[0].Trim().Equals(driveLetter, StringComparison.OrdinalIgnoreCase))
            {
                // Update existing entry
                lines[i] = newEntry;
                entryFound = true;
                break;
            }
        }
        
        if (!entryFound)
        {
            // Add new entry at the end
            lines.Add(newEntry);
        }
        
        // Write back to file
        await File.WriteAllLinesAsync(driveMappingsPath, lines);
    }

    private string BuildInstallArguments(string configPath)
    {
        var args = new StringBuilder();
        args.Append($"-ConfigPath {configPath} ");
        return args.ToString();
    }

    private async Task<ExecutionResult> ExecutePowerShellScript(string scriptPath, string arguments, int timeoutSeconds = 300)
    {
        string? tempOutputFile = null;
        string? tempErrorFile = null;
        string output;
        string error;

        try
        {
            // Non-elevated: use stream redirection
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -NoProfile -NonInteractive -File \"{scriptPath}\" {arguments}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            using var process = new Process { StartInfo = startInfo };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) => {
                if (e.Data != null) outputBuilder.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (sender, e) => {
                if (e.Data != null) errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            await process.WaitForExitAsync(cts.Token);

            return new ExecutionResult
            {
                Success = process.ExitCode == 0,
                Output = outputBuilder.ToString(),
                Error = errorBuilder.ToString(),
                ExitCode = process.ExitCode
            };
        }
        catch (OperationCanceledException)
        {
            // Read output from temp files
            output = File.Exists(tempOutputFile) ? await File.ReadAllTextAsync(tempOutputFile) : string.Empty;
            error = File.Exists(tempErrorFile) ? await File.ReadAllTextAsync(tempErrorFile) : string.Empty;

            return new ExecutionResult
            {
                Success = false,
                Error = $"Script execution timed out after {timeoutSeconds} seconds",
                ExitCode = -1
            };
        }
    }

    private async Task<ExecutionResult> ExecutePowerShellCommand(string command, int timeoutSeconds = 30)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{command}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };

            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (sender, e) => {
                if (e.Data != null) output.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (sender, e) => {
                if (e.Data != null) error.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            await process.WaitForExitAsync(cts.Token);

            return new ExecutionResult
            {
                Success = process.ExitCode == 0,
                Output = output.ToString(),
                Error = error.ToString(),
                ExitCode = process.ExitCode
            };
        }
        catch (OperationCanceledException)
        {
            return new ExecutionResult
            {
                Success = false,
                Error = $"Command execution timed out after {timeoutSeconds} seconds",
                ExitCode = -1
            };
        }
    }

    private bool CheckDriveExists(string mountName, string letter)
    {
        try
        {
            // Can't check DriveInfo from elevated context - drives in user session aren't visible
            // Instead, check for running rclone process with our mount name and drive letter
            var rcloneProcesses = Process.GetProcessesByName("rclone");
            
            foreach (var proc in rcloneProcesses)
            {
                try
                {
                    var cmdLine = GetProcessCommandLine(proc);
                    if (!string.IsNullOrEmpty(cmdLine) && 
                        cmdLine.Contains(mountName, StringComparison.OrdinalIgnoreCase) &&
                        cmdLine.Contains(letter, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                catch
                {
                    // Access denied or process exited - continue checking others
                }
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }
    
    private string GetProcessCommandLine(Process process)
    {
        try
        {
            // Use WMI to get full command line
            using var searcher = new System.Management.ManagementObjectSearcher(
                $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}");
            using var results = searcher.Get();
            
            foreach (System.Management.ManagementObject obj in results)
            {
                return obj["CommandLine"]?.ToString() ?? string.Empty;
            }
            
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}