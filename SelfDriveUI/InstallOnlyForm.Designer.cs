namespace SelfDriveInstaller
{
    partial class InstallOnlyForm
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
            grpCredentials = new GroupBox();
            btnTest = new Button();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            btnShowPassword = new Button();
            grpActions = new GroupBox();
            button1 = new Button();
            btnInstall = new Button();
            grpStatusAndLogs = new GroupBox();
            txtLogs = new TextBox();
            loadConfig = new Button();
            pnlBanner.SuspendLayout();
            grpCredentials.SuspendLayout();
            grpActions.SuspendLayout();
            grpStatusAndLogs.SuspendLayout();
            SuspendLayout();
            // 
            // pnlBanner
            // 
            pnlBanner.BackColor = Color.FromArgb(240, 240, 240);
            pnlBanner.Controls.Add(lblTitle);
            pnlBanner.Dock = DockStyle.Top;
            pnlBanner.Location = new Point(0, 0);
            pnlBanner.Name = "pnlBanner";
            pnlBanner.Size = new Size(450, 38);
            pnlBanner.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Location = new Point(107, 2);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(236, 36);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Self Drive Installer";
            // 
            // grpCredentials
            // 
            grpCredentials.Controls.Add(btnTest);
            grpCredentials.Controls.Add(lblUsername);
            grpCredentials.Controls.Add(txtUsername);
            grpCredentials.Controls.Add(lblPassword);
            grpCredentials.Controls.Add(txtPassword);
            grpCredentials.Controls.Add(btnShowPassword);
            grpCredentials.Location = new Point(12, 40);
            grpCredentials.Name = "grpCredentials";
            grpCredentials.Size = new Size(426, 80);
            grpCredentials.TabIndex = 1;
            grpCredentials.TabStop = false;
            grpCredentials.Text = "Credentials";
            // 
            // btnTest
            // 
            btnTest.Location = new Point(361, 17);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(49, 25);
            btnTest.TabIndex = 5;
            btnTest.Text = "Test";
            btnTest.UseVisualStyleBackColor = true;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(6, 20);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(63, 15);
            lblUsername.TabIndex = 0;
            lblUsername.Text = "Username:";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(80, 17);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(275, 23);
            txtUsername.TabIndex = 1;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(6, 50);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(60, 15);
            lblPassword.TabIndex = 2;
            lblPassword.Text = "Password:";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(80, 47);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(275, 23);
            txtPassword.TabIndex = 3;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnShowPassword
            // 
            btnShowPassword.Location = new Point(361, 47);
            btnShowPassword.Name = "btnShowPassword";
            btnShowPassword.Size = new Size(49, 23);
            btnShowPassword.TabIndex = 4;
            btnShowPassword.Text = "Show";
            btnShowPassword.UseVisualStyleBackColor = true;
            // 
            // grpActions
            // 
            grpActions.Controls.Add(loadConfig);
            grpActions.Controls.Add(button1);
            grpActions.Controls.Add(btnInstall);
            grpActions.Location = new Point(12, 126);
            grpActions.Name = "grpActions";
            grpActions.Size = new Size(426, 54);
            grpActions.TabIndex = 2;
            grpActions.TabStop = false;
            grpActions.Text = "Actions";
            // 
            // button1
            // 
            button1.Location = new Point(141, 20);
            button1.Name = "button1";
            button1.Size = new Size(96, 25);
            button1.TabIndex = 2;
            button1.Text = "Update Status";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // btnInstall
            // 
            btnInstall.Location = new Point(15, 20);
            btnInstall.Name = "btnInstall";
            btnInstall.Size = new Size(120, 25);
            btnInstall.TabIndex = 0;
            btnInstall.Text = "Install && Configure";
            btnInstall.UseVisualStyleBackColor = true;
            // 
            // grpStatusAndLogs
            // 
            grpStatusAndLogs.Controls.Add(txtLogs);
            grpStatusAndLogs.Location = new Point(12, 186);
            grpStatusAndLogs.Name = "grpStatusAndLogs";
            grpStatusAndLogs.Size = new Size(426, 163);
            grpStatusAndLogs.TabIndex = 3;
            grpStatusAndLogs.TabStop = false;
            grpStatusAndLogs.Text = "Status && Logs";
            // 
            // txtLogs
            // 
            txtLogs.Location = new Point(6, 20);
            txtLogs.Multiline = true;
            txtLogs.Name = "txtLogs";
            txtLogs.ReadOnly = true;
            txtLogs.ScrollBars = ScrollBars.Vertical;
            txtLogs.Size = new Size(414, 137);
            txtLogs.TabIndex = 0;
            // 
            // loadConfig
            // 
            loadConfig.Location = new Point(243, 20);
            loadConfig.Name = "loadConfig";
            loadConfig.Size = new Size(96, 25);
            loadConfig.TabIndex = 3;
            loadConfig.Text = "Load Config";
            loadConfig.UseVisualStyleBackColor = true;
            loadConfig.Click += loadConfig_Click;
            // 
            // InstallOnlyForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(450, 361);
            Controls.Add(pnlBanner);
            Controls.Add(grpCredentials);
            Controls.Add(grpActions);
            Controls.Add(grpStatusAndLogs);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimumSize = new Size(466, 400);
            Name = "InstallOnlyForm";
            ShowIcon = false;
            Text = "Self Drive - Installer";
            pnlBanner.ResumeLayout(false);
            pnlBanner.PerformLayout();
            grpCredentials.ResumeLayout(false);
            grpCredentials.PerformLayout();
            grpActions.ResumeLayout(false);
            grpStatusAndLogs.ResumeLayout(false);
            grpStatusAndLogs.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlBanner;
        private Label lblTitle;
        private GroupBox grpCredentials;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnShowPassword;
        private GroupBox grpActions;
        private Button btnInstall;
        private GroupBox grpStatusAndLogs;
        private TextBox txtLogs;
        private Button button1;
        private Button btnTest;
        private Button loadConfig;
    }
}