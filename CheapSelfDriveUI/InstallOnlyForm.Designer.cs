namespace CheapSelfDriveUI
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
            lblSubtitle = new Label();
            grpCredentials = new GroupBox();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            btnShowPassword = new Button();
            grpActions = new GroupBox();
            btnInstall = new Button();
            btnTest = new Button();
            grpStatusAndLogs = new GroupBox();
            txtLogs = new TextBox();
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
            pnlBanner.Controls.Add(lblSubtitle);
            pnlBanner.Dock = DockStyle.Top;
            pnlBanner.Location = new Point(0, 0);
            pnlBanner.Name = "pnlBanner";
            pnlBanner.Size = new Size(450, 60);
            pnlBanner.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(44, 62, 80);
            lblTitle.Location = new Point(15, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(331, 36);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Cheap Self Drive Manager";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 10F);
            lblSubtitle.ForeColor = Color.FromArgb(127, 140, 141);
            lblSubtitle.Location = new Point(29, 44);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(103, 19);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Cloud drive like";
            // 
            // grpCredentials
            // 
            grpCredentials.Controls.Add(lblUsername);
            grpCredentials.Controls.Add(txtUsername);
            grpCredentials.Controls.Add(lblPassword);
            grpCredentials.Controls.Add(txtPassword);
            grpCredentials.Controls.Add(btnShowPassword);
            grpCredentials.Location = new Point(12, 70);
            grpCredentials.Name = "grpCredentials";
            grpCredentials.Size = new Size(420, 80);
            grpCredentials.TabIndex = 1;
            grpCredentials.TabStop = false;
            grpCredentials.Text = "Credentials";
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
            txtUsername.Size = new Size(330, 23);
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
            txtPassword.Size = new Size(300, 23);
            txtPassword.TabIndex = 3;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnShowPassword
            // 
            btnShowPassword.Location = new Point(385, 47);
            btnShowPassword.Name = "btnShowPassword";
            btnShowPassword.Size = new Size(25, 23);
            btnShowPassword.TabIndex = 4;
            btnShowPassword.Text = "👁";
            btnShowPassword.UseVisualStyleBackColor = true;
            // 
            // grpActions
            // 
            grpActions.Controls.Add(btnInstall);
            grpActions.Controls.Add(btnTest);
            grpActions.Location = new Point(12, 160);
            grpActions.Name = "grpActions";
            grpActions.Size = new Size(420, 54);
            grpActions.TabIndex = 2;
            grpActions.TabStop = false;
            grpActions.Text = "Actions";
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
            // btnTest
            // 
            btnTest.Location = new Point(145, 20);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(80, 25);
            btnTest.TabIndex = 1;
            btnTest.Text = "Test";
            btnTest.UseVisualStyleBackColor = true;
            // 
            // grpStatusAndLogs
            // 
            grpStatusAndLogs.Controls.Add(txtLogs);
            grpStatusAndLogs.Location = new Point(12, 222);
            grpStatusAndLogs.Name = "grpStatusAndLogs";
            grpStatusAndLogs.Size = new Size(420, 129);
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
            txtLogs.Size = new Size(408, 103);
            txtLogs.TabIndex = 0;
            txtLogs.TextChanged += txtLogs_TextChanged;
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
            Text = "Cheap Self Drive - Installer";
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
        private Label lblSubtitle;
        private GroupBox grpCredentials;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnShowPassword;
        private GroupBox grpActions;
        private Button btnInstall;
        private Button btnTest;
        private GroupBox grpStatusAndLogs;
        private TextBox txtLogs;
    }
}