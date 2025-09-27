namespace CheapSelfDriveUI
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlBanner = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            grpMountSettings = new GroupBox();
            lblMountName = new Label();
            txtMountName = new TextBox();
            lblDriveLetter = new Label();
            txtDriveLetter = new TextBox();
            grpSFTPConnection = new GroupBox();
            lblNASAddress = new Label();
            txtNASAddress = new TextBox();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            btnShowPassword = new Button();
            lblPort = new Label();
            txtPort = new TextBox();
            lblRemotePath = new Label();
            txtRemotePath = new TextBox();
            grpPathsAndCaching = new GroupBox();
            lblVFSCache = new Label();
            txtVFSCache = new TextBox();
            lblLogFile = new Label();
            txtLogFile = new TextBox();
            txtRcloneDir = new TextBox();
            grpAdvancedSettings = new GroupBox();
            lblCacheMode = new Label();
            cmbCacheMode = new ComboBox();
            lblCacheSize = new Label();
            txtCacheSize = new TextBox();
            lblCacheAge = new Label();
            txtCacheAge = new TextBox();
            lblPollInterval = new Label();
            txtPollInterval = new TextBox();
            grpActions = new GroupBox();
            btnInstall = new Button();
            btnUninstall = new Button();
            btnStartMount = new Button();
            btnTest = new Button();
            grpStatusAndLogs = new GroupBox();
            lblStatus = new Label();
            txtLogs = new TextBox();
            pnlBottomButtons = new Panel();
            btnSaveConfig = new Button();
            btnLoadConfig = new Button();
            btnResetDefaults = new Button();
            btnAbout = new Button();
            pnlBanner.SuspendLayout();
            grpMountSettings.SuspendLayout();
            grpSFTPConnection.SuspendLayout();
            grpPathsAndCaching.SuspendLayout();
            grpAdvancedSettings.SuspendLayout();
            grpActions.SuspendLayout();
            grpStatusAndLogs.SuspendLayout();
            pnlBottomButtons.SuspendLayout();
            SuspendLayout();
            // 
            // pnlBanner
            // 
            pnlBanner.BackColor = Color.FromArgb(240, 240, 240);
            pnlBanner.Controls.Add(lblTitle);
            pnlBanner.Controls.Add(lblSubtitle);
            pnlBanner.Dock = DockStyle.Top;
            pnlBanner.Location = new Point(0, 0);
            pnlBanner.Name = "pnlBanner";
            pnlBanner.Size = new Size(822, 80);
            pnlBanner.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Location = new Point(20, 15);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(310, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Cheap Self Drive Manager";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 12F);
            lblSubtitle.ForeColor = Color.FromArgb(127, 140, 141);
            lblSubtitle.Location = new Point(30, 45);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(203, 21);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "SFTP based Cloud Drive like";
            // 
            // grpMountSettings
            // 
            grpMountSettings.Controls.Add(lblMountName);
            grpMountSettings.Controls.Add(txtMountName);
            grpMountSettings.Controls.Add(lblDriveLetter);
            grpMountSettings.Controls.Add(txtDriveLetter);
            grpMountSettings.Location = new Point(12, 95);
            grpMountSettings.Name = "grpMountSettings";
            grpMountSettings.Size = new Size(200, 130);
            grpMountSettings.TabIndex = 1;
            grpMountSettings.TabStop = false;
            grpMountSettings.Text = "Mount Settings";
            // 
            // lblMountName
            // 
            lblMountName.AutoSize = true;
            lblMountName.Location = new Point(6, 25);
            lblMountName.Name = "lblMountName";
            lblMountName.Size = new Size(81, 15);
            lblMountName.TabIndex = 0;
            lblMountName.Text = "Mount Name:";
            // 
            // txtMountName
            // 
            txtMountName.Location = new Point(6, 45);
            txtMountName.Name = "txtMountName";
            txtMountName.Size = new Size(180, 23);
            txtMountName.TabIndex = 1;
            // 
            // lblDriveLetter
            // 
            lblDriveLetter.AutoSize = true;
            lblDriveLetter.Location = new Point(6, 75);
            lblDriveLetter.Name = "lblDriveLetter";
            lblDriveLetter.Size = new Size(70, 15);
            lblDriveLetter.TabIndex = 2;
            lblDriveLetter.Text = "Drive Letter:";
            // 
            // txtDriveLetter
            // 
            txtDriveLetter.Location = new Point(6, 95);
            txtDriveLetter.Name = "txtDriveLetter";
            txtDriveLetter.Size = new Size(50, 23);
            txtDriveLetter.TabIndex = 3;
            // 
            // grpSFTPConnection
            // 
            grpSFTPConnection.Controls.Add(lblNASAddress);
            grpSFTPConnection.Controls.Add(txtNASAddress);
            grpSFTPConnection.Controls.Add(lblUsername);
            grpSFTPConnection.Controls.Add(txtUsername);
            grpSFTPConnection.Controls.Add(lblPassword);
            grpSFTPConnection.Controls.Add(txtPassword);
            grpSFTPConnection.Controls.Add(btnShowPassword);
            grpSFTPConnection.Controls.Add(lblPort);
            grpSFTPConnection.Controls.Add(txtPort);
            grpSFTPConnection.Controls.Add(lblRemotePath);
            grpSFTPConnection.Controls.Add(txtRemotePath);
            grpSFTPConnection.Location = new Point(220, 95);
            grpSFTPConnection.Name = "grpSFTPConnection";
            grpSFTPConnection.Size = new Size(300, 205);
            grpSFTPConnection.TabIndex = 2;
            grpSFTPConnection.TabStop = false;
            grpSFTPConnection.Text = "SFTP Connection";
            // 
            // lblNASAddress
            // 
            lblNASAddress.AutoSize = true;
            lblNASAddress.Location = new Point(6, 25);
            lblNASAddress.Name = "lblNASAddress";
            lblNASAddress.Size = new Size(78, 15);
            lblNASAddress.TabIndex = 0;
            lblNASAddress.Text = "NAS Address:";
            // 
            // txtNASAddress
            // 
            txtNASAddress.Location = new Point(6, 45);
            txtNASAddress.Name = "txtNASAddress";
            txtNASAddress.Size = new Size(280, 23);
            txtNASAddress.TabIndex = 1;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(6, 75);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(63, 15);
            lblUsername.TabIndex = 2;
            lblUsername.Text = "Username:";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(6, 95);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(280, 23);
            txtUsername.TabIndex = 3;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(6, 125);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(60, 15);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "Password:";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(6, 145);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(250, 23);
            txtPassword.TabIndex = 5;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnShowPassword
            // 
            btnShowPassword.Location = new Point(260, 145);
            btnShowPassword.Name = "btnShowPassword";
            btnShowPassword.Size = new Size(26, 23);
            btnShowPassword.TabIndex = 6;
            btnShowPassword.Text = "👁";
            btnShowPassword.UseVisualStyleBackColor = true;
            // 
            // lblPort
            // 
            lblPort.AutoSize = true;
            lblPort.Location = new Point(6, 175);
            lblPort.Name = "lblPort";
            lblPort.Size = new Size(32, 15);
            lblPort.TabIndex = 7;
            lblPort.Text = "Port:";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(44, 172);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(22, 23);
            txtPort.TabIndex = 8;
            txtPort.Text = "22";
            // 
            // lblRemotePath
            // 
            lblRemotePath.AutoSize = true;
            lblRemotePath.Location = new Point(72, 175);
            lblRemotePath.Name = "lblRemotePath";
            lblRemotePath.Size = new Size(78, 15);
            lblRemotePath.TabIndex = 9;
            lblRemotePath.Text = "Remote Path:";
            // 
            // txtRemotePath
            // 
            txtRemotePath.Location = new Point(150, 172);
            txtRemotePath.Name = "txtRemotePath";
            txtRemotePath.Size = new Size(136, 23);
            txtRemotePath.TabIndex = 10;
            // 
            // grpPathsAndCaching
            // 
            grpPathsAndCaching.Controls.Add(lblVFSCache);
            grpPathsAndCaching.Controls.Add(txtVFSCache);
            grpPathsAndCaching.Controls.Add(lblLogFile);
            grpPathsAndCaching.Controls.Add(txtLogFile);
            grpPathsAndCaching.Controls.Add(txtRcloneDir);
            grpPathsAndCaching.Location = new Point(12, 231);
            grpPathsAndCaching.Name = "grpPathsAndCaching";
            grpPathsAndCaching.Size = new Size(200, 128);
            grpPathsAndCaching.TabIndex = 3;
            grpPathsAndCaching.TabStop = false;
            grpPathsAndCaching.Text = "Paths";
            // 
            // lblVFSCache
            // 
            lblVFSCache.AutoSize = true;
            lblVFSCache.Location = new Point(6, 25);
            lblVFSCache.Name = "lblVFSCache";
            lblVFSCache.Size = new Size(65, 15);
            lblVFSCache.TabIndex = 0;
            lblVFSCache.Text = "VFS Cache:";
            // 
            // txtVFSCache
            // 
            txtVFSCache.Location = new Point(6, 45);
            txtVFSCache.Name = "txtVFSCache";
            txtVFSCache.Size = new Size(180, 23);
            txtVFSCache.TabIndex = 1;
            // 
            // lblLogFile
            // 
            lblLogFile.AutoSize = true;
            lblLogFile.Location = new Point(6, 75);
            lblLogFile.Name = "lblLogFile";
            lblLogFile.Size = new Size(51, 15);
            lblLogFile.TabIndex = 2;
            lblLogFile.Text = "Log File:";
            // 
            // txtLogFile
            // 
            txtLogFile.Location = new Point(6, 95);
            txtLogFile.Name = "txtLogFile";
            txtLogFile.Size = new Size(180, 23);
            txtLogFile.TabIndex = 3;
            // 
            // txtRcloneDir
            // 
            txtRcloneDir.Location = new Point(6, 145);
            txtRcloneDir.Name = "txtRcloneDir";
            txtRcloneDir.Size = new Size(180, 23);
            txtRcloneDir.TabIndex = 5;
            // 
            // grpAdvancedSettings
            // 
            grpAdvancedSettings.Controls.Add(lblCacheMode);
            grpAdvancedSettings.Controls.Add(cmbCacheMode);
            grpAdvancedSettings.Controls.Add(lblCacheSize);
            grpAdvancedSettings.Controls.Add(txtCacheSize);
            grpAdvancedSettings.Controls.Add(lblCacheAge);
            grpAdvancedSettings.Controls.Add(txtCacheAge);
            grpAdvancedSettings.Controls.Add(lblPollInterval);
            grpAdvancedSettings.Controls.Add(txtPollInterval);
            grpAdvancedSettings.Location = new Point(530, 95);
            grpAdvancedSettings.Name = "grpAdvancedSettings";
            grpAdvancedSettings.Size = new Size(280, 230);
            grpAdvancedSettings.TabIndex = 4;
            grpAdvancedSettings.TabStop = false;
            grpAdvancedSettings.Text = "Advanced Settings";
            // 
            // lblCacheMode
            // 
            lblCacheMode.AutoSize = true;
            lblCacheMode.Location = new Point(6, 25);
            lblCacheMode.Name = "lblCacheMode";
            lblCacheMode.Size = new Size(77, 15);
            lblCacheMode.TabIndex = 0;
            lblCacheMode.Text = "Cache Mode:";
            // 
            // cmbCacheMode
            // 
            cmbCacheMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCacheMode.FormattingEnabled = true;
            cmbCacheMode.Location = new Point(6, 45);
            cmbCacheMode.Name = "cmbCacheMode";
            cmbCacheMode.Size = new Size(260, 23);
            cmbCacheMode.TabIndex = 1;
            // 
            // lblCacheSize
            // 
            lblCacheSize.AutoSize = true;
            lblCacheSize.Location = new Point(6, 75);
            lblCacheSize.Name = "lblCacheSize";
            lblCacheSize.Size = new Size(66, 15);
            lblCacheSize.TabIndex = 2;
            lblCacheSize.Text = "Cache Size:";
            // 
            // txtCacheSize
            // 
            txtCacheSize.Location = new Point(6, 95);
            txtCacheSize.Name = "txtCacheSize";
            txtCacheSize.Size = new Size(260, 23);
            txtCacheSize.TabIndex = 3;
            // 
            // lblCacheAge
            // 
            lblCacheAge.AutoSize = true;
            lblCacheAge.Location = new Point(6, 125);
            lblCacheAge.Name = "lblCacheAge";
            lblCacheAge.Size = new Size(67, 15);
            lblCacheAge.TabIndex = 4;
            lblCacheAge.Text = "Cache Age:";
            // 
            // txtCacheAge
            // 
            txtCacheAge.Location = new Point(6, 145);
            txtCacheAge.Name = "txtCacheAge";
            txtCacheAge.Size = new Size(260, 23);
            txtCacheAge.TabIndex = 5;
            // 
            // lblPollInterval
            // 
            lblPollInterval.AutoSize = true;
            lblPollInterval.Location = new Point(6, 175);
            lblPollInterval.Name = "lblPollInterval";
            lblPollInterval.Size = new Size(72, 15);
            lblPollInterval.TabIndex = 6;
            lblPollInterval.Text = "Poll Interval:";
            // 
            // txtPollInterval
            // 
            txtPollInterval.Location = new Point(6, 195);
            txtPollInterval.Name = "txtPollInterval";
            txtPollInterval.Size = new Size(260, 23);
            txtPollInterval.TabIndex = 7;
            // 
            // grpActions
            // 
            grpActions.Controls.Add(btnInstall);
            grpActions.Controls.Add(btnUninstall);
            grpActions.Controls.Add(btnStartMount);
            grpActions.Controls.Add(btnTest);
            grpActions.Location = new Point(12, 369);
            grpActions.Name = "grpActions";
            grpActions.Size = new Size(798, 60);
            grpActions.TabIndex = 5;
            grpActions.TabStop = false;
            grpActions.Text = "Actions";
            // 
            // btnInstall
            // 
            btnInstall.Location = new Point(20, 25);
            btnInstall.Name = "btnInstall";
            btnInstall.Size = new Size(150, 30);
            btnInstall.TabIndex = 0;
            btnInstall.Text = "Install && Configure";
            btnInstall.UseVisualStyleBackColor = true;
            // 
            // btnUninstall
            // 
            btnUninstall.Location = new Point(180, 25);
            btnUninstall.Name = "btnUninstall";
            btnUninstall.Size = new Size(100, 30);
            btnUninstall.TabIndex = 1;
            btnUninstall.Text = "Uninstall";
            btnUninstall.UseVisualStyleBackColor = true;
            // 
            // btnStartMount
            // 
            btnStartMount.Location = new Point(290, 25);
            btnStartMount.Name = "btnStartMount";
            btnStartMount.Size = new Size(100, 30);
            btnStartMount.TabIndex = 2;
            btnStartMount.Text = "Start Mount";
            btnStartMount.UseVisualStyleBackColor = true;
            // 
            // btnTest
            // 
            btnTest.Location = new Point(400, 25);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(80, 30);
            btnTest.TabIndex = 3;
            btnTest.Text = "Test";
            btnTest.UseVisualStyleBackColor = true;
            // 
            // grpStatusAndLogs
            // 
            grpStatusAndLogs.Controls.Add(lblStatus);
            grpStatusAndLogs.Controls.Add(txtLogs);
            grpStatusAndLogs.Location = new Point(12, 441);
            grpStatusAndLogs.Name = "grpStatusAndLogs";
            grpStatusAndLogs.Size = new Size(798, 150);
            grpStatusAndLogs.TabIndex = 6;
            grpStatusAndLogs.TabStop = false;
            grpStatusAndLogs.Text = "Status && Logs";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatus.Location = new Point(6, 25);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(82, 15);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Status: Ready";
            // 
            // txtLogs
            // 
            txtLogs.Location = new Point(6, 45);
            txtLogs.Multiline = true;
            txtLogs.Name = "txtLogs";
            txtLogs.ReadOnly = true;
            txtLogs.ScrollBars = ScrollBars.Vertical;
            txtLogs.Size = new Size(786, 95);
            txtLogs.TabIndex = 1;
            // 
            // pnlBottomButtons
            // 
            pnlBottomButtons.Controls.Add(btnSaveConfig);
            pnlBottomButtons.Controls.Add(btnLoadConfig);
            pnlBottomButtons.Controls.Add(btnResetDefaults);
            pnlBottomButtons.Controls.Add(btnAbout);
            pnlBottomButtons.Dock = DockStyle.Bottom;
            pnlBottomButtons.Location = new Point(0, 595);
            pnlBottomButtons.Name = "pnlBottomButtons";
            pnlBottomButtons.Size = new Size(822, 50);
            pnlBottomButtons.TabIndex = 7;
            // 
            // btnSaveConfig
            // 
            btnSaveConfig.Location = new Point(12, 10);
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new Size(100, 30);
            btnSaveConfig.TabIndex = 0;
            btnSaveConfig.Text = "Save Config";
            btnSaveConfig.UseVisualStyleBackColor = true;
            // 
            // btnLoadConfig
            // 
            btnLoadConfig.Location = new Point(120, 10);
            btnLoadConfig.Name = "btnLoadConfig";
            btnLoadConfig.Size = new Size(100, 30);
            btnLoadConfig.TabIndex = 1;
            btnLoadConfig.Text = "Load Config";
            btnLoadConfig.UseVisualStyleBackColor = true;
            // 
            // btnResetDefaults
            // 
            btnResetDefaults.Location = new Point(230, 10);
            btnResetDefaults.Name = "btnResetDefaults";
            btnResetDefaults.Size = new Size(120, 30);
            btnResetDefaults.TabIndex = 2;
            btnResetDefaults.Text = "Reset to Defaults";
            btnResetDefaults.UseVisualStyleBackColor = true;
            // 
            // btnAbout
            // 
            btnAbout.Location = new Point(710, 10);
            btnAbout.Name = "btnAbout";
            btnAbout.Size = new Size(80, 30);
            btnAbout.TabIndex = 3;
            btnAbout.Text = "About";
            btnAbout.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(822, 645);
            Controls.Add(pnlBanner);
            Controls.Add(grpMountSettings);
            Controls.Add(grpSFTPConnection);
            Controls.Add(grpPathsAndCaching);
            Controls.Add(grpAdvancedSettings);
            Controls.Add(grpActions);
            Controls.Add(grpStatusAndLogs);
            Controls.Add(pnlBottomButtons);
            MinimumSize = new Size(800, 600);
            Name = "MainForm";
            Text = "CheapSelfDrive Manager";
            pnlBanner.ResumeLayout(false);
            pnlBanner.PerformLayout();
            grpMountSettings.ResumeLayout(false);
            grpMountSettings.PerformLayout();
            grpSFTPConnection.ResumeLayout(false);
            grpSFTPConnection.PerformLayout();
            grpPathsAndCaching.ResumeLayout(false);
            grpPathsAndCaching.PerformLayout();
            grpAdvancedSettings.ResumeLayout(false);
            grpAdvancedSettings.PerformLayout();
            grpActions.ResumeLayout(false);
            grpStatusAndLogs.ResumeLayout(false);
            grpStatusAndLogs.PerformLayout();
            pnlBottomButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlBanner;
        private Label lblTitle;
        private Label lblSubtitle;
        private GroupBox grpMountSettings;
        private Label lblMountName;
        private TextBox txtMountName;
        private Label lblDriveLetter;
        private TextBox txtDriveLetter;
        private GroupBox grpSFTPConnection;
        private Label lblNASAddress;
        private TextBox txtNASAddress;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnShowPassword;
        private Label lblPort;
        private TextBox txtPort;
        private Label lblRemotePath;
        private TextBox txtRemotePath;
        private GroupBox grpPathsAndCaching;
        private Label lblVFSCache;
        private TextBox txtVFSCache;
        private Label lblLogFile;
        private TextBox txtLogFile;
        private TextBox txtRcloneDir;
        private GroupBox grpAdvancedSettings;
        private Label lblCacheMode;
        private ComboBox cmbCacheMode;
        private Label lblCacheSize;
        private TextBox txtCacheSize;
        private Label lblCacheAge;
        private TextBox txtCacheAge;
        private Label lblPollInterval;
        private TextBox txtPollInterval;
        private GroupBox grpActions;
        private Button btnInstall;
        private Button btnUninstall;
        private Button btnStartMount;
        private Button btnTest;
        private GroupBox grpStatusAndLogs;
        private Label lblStatus;
        private TextBox txtLogs;
        private Panel pnlBottomButtons;
        private Button btnSaveConfig;
        private Button btnLoadConfig;
        private Button btnResetDefaults;
        private Button btnAbout;
    }
}