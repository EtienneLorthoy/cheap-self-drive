using CheapSelfDriveUI.Models;
using System.Diagnostics;
using System.Text;

namespace CheapSelfDriveUI.Services;

public class ScriptExecutor
{
    private readonly string _scriptDirectory;

    public ScriptExecutor(string scriptDirectory)
    {
        _scriptDirectory = scriptDirectory;
    }

    public async Task<ExecutionResult> RunInstallScript(Config config, string password)
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

            var arguments = BuildInstallArguments(config, password);
            return await ExecutePowerShellScript(scriptPath, arguments, requireElevation: true);
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

    public async Task<ExecutionResult> RunUninstallScript(string mountName)
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
            return await ExecutePowerShellScript(scriptPath, arguments, requireElevation: true);
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

    public async Task<ExecutionResult> RunStartScript(string mountName)
    {
        try
        {
            var scriptPath = Path.Combine(_scriptDirectory, "Start-SFTPDiskScheduledTask.ps1");
            if (!File.Exists(scriptPath))
            {
                return new ExecutionResult
                {
                    Success = false,
                    Error = $"Start script not found at: {scriptPath}"
                };
            }

            var arguments = $"-MountName '{mountName}'";
            return await ExecutePowerShellScript(scriptPath, arguments, requireElevation: false);
        }
        catch (Exception ex)
        {
            return new ExecutionResult
            {
                Success = false,
                Error = $"Failed to run start script: {ex.Message}"
            };
        }
    }

    public async Task<ExecutionResult> TestConnection(Config config, string password)
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

    public MountStatus GetMountStatus(string mountName)
    {
        try
        {
            // Check if scheduled task exists
            var taskExists = CheckScheduledTaskExists(mountName);
            
            // Check if drive is mounted
            var driveExists = CheckDriveExists(mountName);

            if (driveExists)
                return MountStatus.Running;
            else if (taskExists)
                return MountStatus.Installed;
            else
                return MountStatus.NotConfigured;
        }
        catch
        {
            return MountStatus.Error;
        }
    }

    private string BuildInstallArguments(Config config, string password)
    {
        var args = new StringBuilder();
        args.Append($"-MountName '{config.MountName}' ");
        args.Append($"-DriveLetter '{config.DriveLetter}' ");
        args.Append($"-NASAddress '{config.NASAddress}' ");
        args.Append($"-NASUsername '{config.NASUsername}' ");
        args.Append($"-NASPassword '{password}' ");
        args.Append($"-NASPort {config.NASPort} ");
        args.Append($"-NASAbsolutePath '{config.NASAbsolutePath}' ");
        args.Append($"-ShellType '{config.ShellType}' ");
        args.Append($"-VFSCacheDir '{config.GetExpandedVFSCacheDir()}' ");
        args.Append($"-RcloneLogs '{config.GetExpandedRcloneLogs()}' ");
        args.Append($"-RcloneConfigDir '{config.GetExpandedRcloneConfigDir()}'");

        return args.ToString();
    }

    private async Task<ExecutionResult> ExecutePowerShellScript(string scriptPath, string arguments, bool requireElevation)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" {arguments}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (requireElevation)
        {
            startInfo.UseShellExecute = true;
            startInfo.Verb = "runas";
            startInfo.RedirectStandardOutput = false;
            startInfo.RedirectStandardError = false;
        }

        using var process = new Process { StartInfo = startInfo };
        
        var output = new StringBuilder();
        var error = new StringBuilder();

        if (!requireElevation)
        {
            process.OutputDataReceived += (sender, e) => {
                if (e.Data != null) output.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (sender, e) => {
                if (e.Data != null) error.AppendLine(e.Data);
            };
        }

        process.Start();

        if (!requireElevation)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        await process.WaitForExitAsync();

        return new ExecutionResult
        {
            Success = process.ExitCode == 0,
            Output = output.ToString(),
            Error = error.ToString(),
            ExitCode = process.ExitCode
        };
    }

    private async Task<ExecutionResult> ExecutePowerShellCommand(string command)
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

        await process.WaitForExitAsync();

        return new ExecutionResult
        {
            Success = process.ExitCode == 0,
            Output = output.ToString(),
            Error = error.ToString(),
            ExitCode = process.ExitCode
        };
    }

    private bool CheckScheduledTaskExists(string mountName)
    {
        try
        {
            var taskName = $"SFTP Mount - {mountName}";
            var startInfo = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = $"/query /tn \"{taskName}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private bool CheckDriveExists(string mountName)
    {
        try
        {
            // This is a simplified check - in reality, we'd need to check if the specific mount is active
            var drives = DriveInfo.GetDrives();
            return drives.Any(d => d.IsReady && d.DriveType == DriveType.Network);
        }
        catch
        {
            return false;
        }
    }
}