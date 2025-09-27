using CheapSelfDriveUI.Models;
using CheapSelfDriveUI.Services;

namespace CheapSelfDriveUI;

public partial class InstallOnlyForm : Form
{
    private readonly ConfigurationManager _configManager;
    private readonly PasswordManager _passwordManager;
    private readonly ScriptExecutor _scriptExecutor;
    private Config _currentConfig;
    private BannerConfig _bannerConfig = new();
    private string _currentPassword = "";

    public InstallOnlyForm()
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
    }

    private void txtLogs_TextChanged(object sender, EventArgs e)
    {

    }
}