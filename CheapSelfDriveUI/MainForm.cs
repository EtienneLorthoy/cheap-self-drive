using CheapSelfDriveUI.Models;
using CheapSelfDriveUI.Services;

namespace CheapSelfDriveUI;

public partial class MainForm : Form
{
    private readonly ConfigurationManager _configManager;
    private readonly PasswordManager _passwordManager;
    private readonly ScriptExecutor _scriptExecutor;
    private Config _currentConfig;
    private BannerConfig _bannerConfig = new();
    private string _currentPassword = "";

    public MainForm()
    {
        InitializeComponent();

        _configManager = new ConfigurationManager();
        _passwordManager = new PasswordManager();
        _scriptExecutor = new ScriptExecutor(GetScriptDirectory());
        _currentConfig = _configManager.GetDefaultConfiguration();

        InitializeUI();
        LoadConfiguration();
        LoadBannerConfiguration();
        UpdateBanner();
        UpdateStatus();
    }

    private void InitializeUI()
    {
        // Set form properties
        this.Text = "CheapSelfDrive Manager";
        this.Size = new Size(850, 700);
        this.MinimumSize = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.Sizable;

        // Enable visual styles
        Application.EnableVisualStyles();
        //Application.SetCompatibleTextRenderingDefault(false);

        // Set font
        this.Font = new Font("Segoe UI", 9F);

        // Initialize cache mode combo box
        cmbCacheMode.Items.AddRange(new[] { "off", "minimal", "writes", "full" });
        cmbCacheMode.SelectedIndex = 3; // "full"

        // Set password field properties
        txtPassword.UseSystemPasswordChar = true;

        // Wire up events
        btnShowPassword.Click += BtnShowPassword_Click;
        btnInstall.Click += BtnInstall_Click;
        btnUninstall.Click += BtnUninstall_Click;
        btnStartMount.Click += BtnStartMount_Click;
        btnTest.Click += BtnTest_Click;
        btnSaveConfig.Click += BtnSaveConfig_Click;
        btnLoadConfig.Click += BtnLoadConfig_Click;
        btnResetDefaults.Click += BtnResetDefaults_Click;
        btnAbout.Click += BtnAbout_Click;

        // Update UI from current config
        PopulateUI(_currentConfig);
    }

    private void LoadConfiguration()
    {
        try
        {
            var configPath = Path.Combine(Application.StartupPath, "config.json");
            if (File.Exists(configPath))
            {
                _currentConfig = _configManager.LoadConfiguration(configPath);
                PopulateUI(_currentConfig);

                // Try to load password
                var passwordId = _passwordManager.GetPasswordIdentifier(
                    _currentConfig.MountName,
                    _currentConfig.NASUsername,
                    _currentConfig.NASAddress);

                if (_passwordManager.HasStoredPassword(passwordId))
                {
                    _currentPassword = _passwordManager.RetrievePassword(passwordId);
                    txtPassword.Text = _currentPassword;
                }
            }

            UpdateStatus("Configuration loaded successfully", LogLevel.Success);
        }
        catch (Exception ex)
        {
            UpdateStatus($"Failed to load configuration: {ex.Message}", LogLevel.Error);
        }
    }

    private void LoadBannerConfiguration()
    {
        try
        {
            var bannerPath = Path.Combine(Application.StartupPath, "banner-default.json");
            _bannerConfig = _configManager.LoadBannerConfiguration(bannerPath);
        }
        catch
        {
            _bannerConfig = new BannerConfig();
        }
    }

    private void UpdateBanner()
    {
        try
        {
            pnlBanner.BackColor = ColorTranslator.FromHtml(_bannerConfig.BackgroundColor);
            lblTitle.Text = _bannerConfig.Title;
            lblSubtitle.Text = _bannerConfig.Subtitle;
            lblTitle.ForeColor = ColorTranslator.FromHtml(_bannerConfig.TitleColor);
            lblSubtitle.ForeColor = ColorTranslator.FromHtml(_bannerConfig.SubtitleColor);

            lblTitle.Font = new Font(_bannerConfig.TitleFont, _bannerConfig.TitleSize, FontStyle.Bold);
            lblSubtitle.Font = new Font(_bannerConfig.SubtitleFont, _bannerConfig.SubtitleSize);
        }
        catch
        {
            // Fallback to defaults if banner config is invalid
            lblTitle.Text = "CheapSelfDrive Manager";
            lblSubtitle.Text = "SFTP Drive Mounting Made Simple";
        }
    }

    public void PopulateUI(Config config)
    {
        txtMountName.Text = config.MountName;
        txtDriveLetter.Text = config.DriveLetter;
        txtNASAddress.Text = config.NASAddress;
        txtUsername.Text = config.NASUsername;
        txtPort.Text = config.NASPort.ToString();
        txtRemotePath.Text = config.NASAbsolutePath;
        txtVFSCache.Text = config.VFSCacheDir;
        txtLogFile.Text = config.RcloneLogs;
        txtRcloneDir.Text = config.RcloneConfigDir;

        // Advanced settings
        cmbCacheMode.Text = config.AdvancedSettings.CacheMode;
        txtCacheSize.Text = config.AdvancedSettings.CacheMaxSize;
        txtCacheAge.Text = config.AdvancedSettings.CacheMaxAge;
        txtPollInterval.Text = config.AdvancedSettings.PollInterval;
    }

    public Config ExtractConfiguration()
    {
        var config = new Config
        {
            MountName = txtMountName.Text.Trim(),
            DriveLetter = txtDriveLetter.Text.Trim(),
            NASAddress = txtNASAddress.Text.Trim(),
            NASUsername = txtUsername.Text.Trim(),
            NASAbsolutePath = txtRemotePath.Text.Trim(),
            VFSCacheDir = txtVFSCache.Text.Trim(),
            RcloneLogs = txtLogFile.Text.Trim(),
            RcloneConfigDir = txtRcloneDir.Text.Trim(),
            AdvancedSettings = new AdvancedSettings
            {
                CacheMode = cmbCacheMode.Text,
                CacheMaxSize = txtCacheSize.Text.Trim(),
                CacheMaxAge = txtCacheAge.Text.Trim(),
                PollInterval = txtPollInterval.Text.Trim()
            }
        };

        if (int.TryParse(txtPort.Text.Trim(), out int port))
        {
            config.NASPort = port;
        }

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

        // Update status label
        lblStatus.Text = $"Status: {message}";
        lblStatus.ForeColor = level switch
        {
            LogLevel.Error => Color.Red,
            LogLevel.Warning => Color.Orange,
            LogLevel.Success => Color.Green,
            _ => Color.Black
        };

        Application.DoEvents();
    }

    private void UpdateStatus()
    {
        try
        {
            var status = _scriptExecutor.GetMountStatus(_currentConfig.MountName);
            var statusText = status switch
            {
                MountStatus.NotConfigured => "Not Configured",
                MountStatus.Configured => "Configured",
                MountStatus.Installed => "Installed",
                MountStatus.Running => "Running",
                MountStatus.Error => "Error",
                _ => "Unknown"
            };

            UpdateStatus($"Mount status: {statusText}");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Failed to check status: {ex.Message}", LogLevel.Error);
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
            _currentConfig = ExtractConfiguration();
            _configManager.ValidateConfiguration(_currentConfig);

            _currentPassword = txtPassword.Text;
            if (string.IsNullOrEmpty(_currentPassword))
            {
                UpdateStatus("Password is required for installation", LogLevel.Error);
                return;
            }

            EnableControls(false);
            UpdateStatus("Starting installation...", LogLevel.Info);

            var result = await _scriptExecutor.RunInstallScript(_currentConfig, _currentPassword);

            if (result.Success)
            {
                UpdateStatus("Installation completed successfully", LogLevel.Success);

                // Save password
                var passwordId = _passwordManager.GetPasswordIdentifier(
                    _currentConfig.MountName,
                    _currentConfig.NASUsername,
                    _currentConfig.NASAddress);
                _passwordManager.StorePassword(passwordId, _currentPassword);

                // Save configuration
                await SaveCurrentConfiguration();
            }
            else
            {
                UpdateStatus($"Installation failed: {result.Error}", LogLevel.Error);
                if (!string.IsNullOrEmpty(result.Output))
                {
                    UpdateStatus($"Output: {result.Output}", LogLevel.Info);
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
            UpdateStatus();
        }
    }

    private async void BtnUninstall_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            $"Are you sure you want to uninstall the SFTP mount '{_currentConfig.MountName}'?",
            "Confirm Uninstall",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
            return;

        try
        {
            EnableControls(false);
            UpdateStatus("Starting uninstallation...", LogLevel.Info);

            var execResult = await _scriptExecutor.RunUninstallScript(_currentConfig.MountName);

            if (execResult.Success)
            {
                UpdateStatus("Uninstallation completed successfully", LogLevel.Success);

                // Delete stored password
                var passwordId = _passwordManager.GetPasswordIdentifier(
                    _currentConfig.MountName,
                    _currentConfig.NASUsername,
                    _currentConfig.NASAddress);
                _passwordManager.DeletePassword(passwordId);
            }
            else
            {
                UpdateStatus($"Uninstallation failed: {execResult.Error}", LogLevel.Error);
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Uninstallation error: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            EnableControls(true);
            UpdateStatus();
        }
    }

    private async void BtnStartMount_Click(object? sender, EventArgs e)
    {
        try
        {
            EnableControls(false);
            UpdateStatus("Starting mount...", LogLevel.Info);

            var result = await _scriptExecutor.RunStartScript(_currentConfig.MountName);

            if (result.Success)
            {
                UpdateStatus("Mount started successfully", LogLevel.Success);
            }
            else
            {
                UpdateStatus($"Failed to start mount: {result.Error}", LogLevel.Error);
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Start mount error: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            EnableControls(true);
            UpdateStatus();
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
                UpdateStatus("Password is required for connection test", LogLevel.Error);
                return;
            }

            EnableControls(false);
            UpdateStatus("Testing connection...", LogLevel.Info);

            var result = await _scriptExecutor.TestConnection(testConfig, testPassword);

            if (result.Success)
            {
                UpdateStatus("Connection test successful", LogLevel.Success);
            }
            else
            {
                UpdateStatus($"Connection test failed: {result.Error}", LogLevel.Error);
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Connection test error: {ex.Message}", LogLevel.Error);
        }
        finally
        {
            EnableControls(true);
        }
    }

    private async void BtnSaveConfig_Click(object? sender, EventArgs e)
    {
        await SaveCurrentConfiguration();
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

    private void BtnLoadConfig_Click(object? sender, EventArgs e)
    {
        LoadConfiguration();
    }

    private void BtnResetDefaults_Click(object? sender, EventArgs e)
    {
        _currentConfig = _configManager.GetDefaultConfiguration();
        PopulateUI(_currentConfig);
        txtPassword.Text = "";
        _currentPassword = "";
        UpdateStatus("Configuration reset to defaults", LogLevel.Info);
    }

    private void BtnAbout_Click(object? sender, EventArgs e)
    {
        var aboutMessage = $@"CheapSelfDrive Manager v1.0.0

A GUI application for managing SFTP drive mounting using rclone.

Features:
• Easy configuration management
• Secure password storage
• Automated installation and setup
• Real-time status monitoring

© 2025 CheapSelfDrive Project";

        MessageBox.Show(aboutMessage, "About CheapSelfDrive Manager",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnShowPassword_Click(object? sender, EventArgs e)
    {
        txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
        btnShowPassword.Text = txtPassword.UseSystemPasswordChar ? "👁" : "🙈";
    }

    public void EnableControls(bool enabled)
    {
        btnInstall.Enabled = enabled;
        btnUninstall.Enabled = enabled;
        btnStartMount.Enabled = enabled;
        btnTest.Enabled = enabled;
        btnSaveConfig.Enabled = enabled;
        btnLoadConfig.Enabled = enabled;
        btnResetDefaults.Enabled = enabled;
    }
}