using SelfDriveInstaller.Models;
using SelfDriveInstaller.Services;

namespace SelfDriveInstaller;

public partial class InstallOnlyForm : Form
{
    private readonly ConfigurationManager _configManager;
    private readonly PasswordManager _passwordManager;
    private readonly ScriptExecutor _scriptExecutor;
    private Config _currentConfig;
    private string _currentPassword = "";

    private string _currentConfigPath = "";

    public InstallOnlyForm()
    {
        InitializeComponent();

        _configManager = new ConfigurationManager();
        _passwordManager = new PasswordManager();
        _scriptExecutor = new ScriptExecutor(GetScriptDirectory());
        _currentConfig = new Config();
        _currentConfigPath = Path.Combine(Application.StartupPath, "config.json");

        InitializeUI();
        LoadConfiguration();
        UpdateStatus();
    }

    private void InitializeUI()
    {
        // Set form properties
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        // Set font
        this.Font = new Font("Segoe UI", 9F);

        // Set password field properties
        txtPassword.UseSystemPasswordChar = true;

        // Wire up events
        btnShowPassword.Click += BtnShowPassword_Click;
        btnInstall.Click += BtnInstall_Click;
        btnTest.Click += BtnTest_Click;

        // Update UI from current config
        PopulateUI(_currentConfig);
    }

    private void LoadConfiguration()
    {
        try
        {
            if (File.Exists(_currentConfigPath))
            {
                _currentConfig = _configManager.LoadConfiguration(_currentConfigPath);
                PopulateUI(_currentConfig);

                // Try to load password

                if (_passwordManager.HasStoredPassword(_currentConfig.MountName))
                {
                    _currentPassword = _passwordManager.RetrievePassword(_currentConfig.MountName);
                    txtPassword.Text = _currentPassword;
                }
                UpdateStatus($"Configuration {_currentConfig.MountName} drive {_currentConfig.DriveLetter} from {_currentConfigPath} loaded successfully", LogLevel.Success);
            }
            else UpdateStatus($"Could not load config from {_currentConfigPath}", LogLevel.Error);

        }
        catch (Exception ex)
        {
            UpdateStatus($"Failed to load configuration: {ex.Message}", LogLevel.Error);
        }
    }

    public void PopulateUI(Config config)
    {
        txtUsername.Text = config.NASUsername;
    }

    public Config ExtractConfiguration()
    {
        var config = _currentConfig; // Use current config as base
        config.NASUsername = txtUsername.Text.Trim();
        return config;
    }

    public void UpdateStatus(string message, LogLevel level = LogLevel.Info)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var prefix = level switch
        {
            LogLevel.Error => "[ERROR]",
            LogLevel.Warning => "[WARNING]",
            LogLevel.Success => "[SUCCESS]",
            _ => "[INFO]"
        };

        var logMessage = $"{timestamp} {prefix} {message}";

        txtLogs.AppendText(logMessage + Environment.NewLine);
        txtLogs.SelectionStart = txtLogs.Text.Length;
        txtLogs.ScrollToCaret();

        Application.DoEvents();
    }

    private void UpdateStatus()
    {
        EnableControls(false);

        try
        {
            var status = _scriptExecutor.GetMountStatus(_currentConfig);
            var statusText = status switch
            {
                MountStatus.NotConfigured => "Not Configured",
                MountStatus.Configured => "Configured",
                MountStatus.Installed => "Installed",
                MountStatus.Running => "Running",
                MountStatus.Error => "Error",
                _ => "Unknown"
            };

            UpdateStatus($"{_currentConfig.MountName} mount status: {statusText}");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Failed to check status: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            EnableControls(true);
        }
    }

    private string GetScriptDirectory()
    {
        var appDir = Application.StartupPath;
        var scriptDir = Path.Combine(appDir, "PowerShell");

        if (Directory.Exists(scriptDir))
            return scriptDir;

        // Fallback to parent directory (for development)
        var parentDir = Directory.GetParent(appDir);
        while (parentDir != null)
        {
            var psDir = Path.Combine(parentDir.FullName, "PowerShell");
            if (Directory.Exists(psDir))
                return psDir;

            // Check for PowerShell scripts in the current directory
            if (Directory.GetFiles(parentDir.FullName, "*.ps1").Any())
                return parentDir.FullName;

            parentDir = parentDir.Parent;
        }

        return appDir;
    }

    private async void BtnInstall_Click(object? sender, EventArgs e)
    {
        try
        {
            // Save configuration
            await SaveCurrentConfiguration();

            _currentPassword = txtPassword.Text;
            if (string.IsNullOrEmpty(_currentPassword))
            {
                UpdateStatus("Password is required for installation", LogLevel.Error);
                return;
            }

            EnableControls(false);

            // Save password
            UpdateStatus("Storing password securely...", LogLevel.Info);
            _passwordManager.StorePassword(_currentConfig.MountName, _currentPassword);

            // Test connection before proceeding
            UpdateStatus("Validating the credentials...", LogLevel.Info);
            var testResult = await _scriptExecutor.TestConnection(_currentConfig, _currentPassword);
            if (!testResult.Success)
            {
                UpdateStatus($"Credentials validation failed: {testResult.Error}", LogLevel.Error);
                EnableControls(true);
                return;
            }
            else UpdateStatus("Credentials validated successfully", LogLevel.Success);

            UpdateStatus("Running install script: " + _scriptExecutor.GetRunInstallScriptCommandForDebugging(_currentConfigPath), LogLevel.Info);

            var result = await _scriptExecutor.RunInstallScript(_currentConfigPath);

            if (result.Success)
            {
                // Copy all current directory files to C:\\SelfDrive\bin
                UpdateStatus("Copying necessary files...", LogLevel.Info);
                _scriptExecutor.CopyFilesToInstallationDirectory(_currentConfig);

                // Creating the Mount-Drive-SFTPDisk.ps1 script from the template
                UpdateStatus("Creating mount script from template...", LogLevel.Info);
                var mountScriptPath = _scriptExecutor.InstanciatingTemplateScriptMountDrive(_currentConfig);

                // Trying to fire up the drive after installation
                UpdateStatus("Starting the drive...", LogLevel.Info);
                var mountResult = await _scriptExecutor.RunMount(_currentConfig);
                if (mountResult.Success && string.IsNullOrWhiteSpace(mountResult.Error)) UpdateStatus("Drive started successfully.", LogLevel.Success);
                else UpdateStatus($"Failed to start the drive: {mountResult.Output} {mountResult.Error}", LogLevel.Error);

                var status = _scriptExecutor.GetMountStatus(_currentConfig);
                if (status != MountStatus.Running)
                {
                    // Trying to read the log file to see what went wrong
                    var logFilePath = _currentConfig.RcloneLogs.Replace("{MountName}", _currentConfig.MountName);
                    if (File.Exists(logFilePath))
                    {
                        UpdateStatus($"Drive failed to start. Rclone log file at {logFilePath}.", LogLevel.Error);
                    }
                    else UpdateStatus("Drive failed to start.", LogLevel.Error);
                }

                // Adding the application systray to startup
                UpdateStatus("Setting systray as startup application...", LogLevel.Info);
                _scriptExecutor.AddSystrayAppToStartup(_currentConfig);

                UpdateStatus("Configuring systray application...", LogLevel.Info);
                var configSystrayResult = _scriptExecutor.ConfigureSystrayApp(_currentConfig);

                // Finally launch the systray app
                UpdateStatus("Launching systray application...", LogLevel.Info);
                var systrayResult = await _scriptExecutor.LaunchSystrayApp(_currentConfig);
                if (systrayResult.Success && string.IsNullOrWhiteSpace(systrayResult.Error)) UpdateStatus("Systray application launched successfully.", LogLevel.Success);
                else UpdateStatus($"Failed to launch systray application: {systrayResult.Output} {systrayResult.Error}", LogLevel.Error);

                UpdateStatus("Installation completed successfully", LogLevel.Success);
            }
            else
            {
                UpdateStatus($"Installation failed\n: {result.Error}", LogLevel.Error);
                if (!string.IsNullOrEmpty(result.Output))
                {
                    UpdateStatus($"Output:\n{result.Output}", LogLevel.Info);
                }
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Installation error: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            EnableControls(true);
        }
    }

    private async void BtnTest_Click(object? sender, EventArgs e)
    {
        try
        {
            var testConfig = ExtractConfiguration();
            var testPassword = txtPassword.Text;

            if (string.IsNullOrEmpty(testPassword))
            {
                UpdateStatus("Password is required for testing credentials", LogLevel.Error);
                return;
            }

            EnableControls(false);
            UpdateStatus("Testing credentials...", LogLevel.Info);

            var result = await _scriptExecutor.TestConnection(testConfig, testPassword);

            if (result.Success)
            {
                UpdateStatus("Credentials test successful", LogLevel.Success);
            }
            else
            {
                UpdateStatus($"Credentials test failed: {result.Error}", LogLevel.Error);
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Credentials test error: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            EnableControls(true);
        }
    }

    private async Task SaveCurrentConfiguration()
    {
        try
        {
            _currentConfig = ExtractConfiguration();
            _configManager.ValidateConfiguration(_currentConfig);

            var configPath = Path.Combine(Application.StartupPath, "config.json");
            _configManager.SaveConfiguration(_currentConfig, configPath);

            UpdateStatus("Configuration saved successfully", LogLevel.Success);
        }
        catch (Exception ex)
        {
            UpdateStatus($"Failed to save configuration: {ex.Message}", LogLevel.Error);
        }
    }

    private void BtnShowPassword_Click(object? sender, EventArgs e)
    {
        txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
        btnShowPassword.Text = txtPassword.UseSystemPasswordChar ? "👁" : "🙈";
    }

    public void EnableControls(bool enabled)
    {
        btnInstall.Enabled = enabled;
        btnTest.Enabled = enabled;
        button1.Enabled = enabled;
    }

    private void button1_Click(object sender, EventArgs e)
    {
        UpdateStatus();
    }

    private void loadConfig_Click(object sender, EventArgs e)
    {
        using (var openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            openFileDialog.Title = "Select Configuration File";
            openFileDialog.InitialDirectory = Application.StartupPath;
            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    UpdateStatus($"Loading configuration from {openFileDialog.FileName}...", LogLevel.Info);
                    
                    // Try to load and parse the configuration
                    var newConfig = _configManager.LoadConfiguration(openFileDialog.FileName);
                    
                    // Validate the configuration
                    _configManager.ValidateConfiguration(newConfig);
                    
                    // If we get here, the config is valid
                    _currentConfig = newConfig;
                    _currentConfigPath = openFileDialog.FileName;
                    
                    // Update the UI with the new configuration
                    PopulateUI(_currentConfig);
                    
                    // Try to load the password if it exists
                    if (_passwordManager.HasStoredPassword(_currentConfig.MountName))
                    {
                        _currentPassword = _passwordManager.RetrievePassword(_currentConfig.MountName);
                        txtPassword.Text = _currentPassword;
                        UpdateStatus("Stored password loaded successfully", LogLevel.Success);
                    }
                    else
                    {
                        txtPassword.Text = "";
                        _currentPassword = "";
                    }
                    
                    UpdateStatus($"Configuration '{_currentConfig.MountName}' (drive {_currentConfig.DriveLetter}) loaded successfully from {openFileDialog.FileName}", LogLevel.Success);
                    
                    // Update the mount status
                    UpdateStatus();
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Failed to load configuration: {ex.Message}", LogLevel.Error);
                }
            }
        }
    }
}