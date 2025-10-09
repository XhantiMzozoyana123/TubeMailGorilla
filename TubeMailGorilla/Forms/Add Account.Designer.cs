namespace TubeMailGorilla.Forms
{
    partial class Add_Account
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnAdd = new Button();
            ckImapSSL = new CheckBox();
            ckSmtpSSL = new CheckBox();
            txtImapPort = new TextBox();
            lblImapPort = new Label();
            txtImapServer = new TextBox();
            lblImapServer = new Label();
            txtSmtpPort = new TextBox();
            lblsmtpPort = new Label();
            txtSMTPServer = new TextBox();
            lblSmtpHost = new Label();
            txtPassword = new TextBox();
            lblPassword = new Label();
            txtEmail = new TextBox();
            lblEmail = new Label();
            SuspendLayout();
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(493, 293);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(128, 45);
            btnAdd.TabIndex = 29;
            btnAdd.Text = "Add Account";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // ckImapSSL
            // 
            ckImapSSL.AutoSize = true;
            ckImapSSL.Location = new Point(23, 270);
            ckImapSSL.Name = "ckImapSSL";
            ckImapSSL.Size = new Size(142, 24);
            ckImapSSL.TabIndex = 28;
            ckImapSSL.Text = "Enable IMAP SSL";
            ckImapSSL.UseVisualStyleBackColor = true;
            // 
            // ckSmtpSSL
            // 
            ckSmtpSSL.AutoSize = true;
            ckSmtpSSL.Location = new Point(23, 240);
            ckSmtpSSL.Name = "ckSmtpSSL";
            ckSmtpSSL.Size = new Size(144, 24);
            ckSmtpSSL.TabIndex = 27;
            ckSmtpSSL.Text = "Enable SMTP SSL";
            ckSmtpSSL.UseVisualStyleBackColor = true;
            // 
            // txtImapPort
            // 
            txtImapPort.Location = new Point(166, 190);
            txtImapPort.Name = "txtImapPort";
            txtImapPort.Size = new Size(455, 27);
            txtImapPort.TabIndex = 26;
            // 
            // lblImapPort
            // 
            lblImapPort.AutoSize = true;
            lblImapPort.Location = new Point(23, 197);
            lblImapPort.Name = "lblImapPort";
            lblImapPort.RightToLeft = RightToLeft.Yes;
            lblImapPort.Size = new Size(74, 20);
            lblImapPort.TabIndex = 25;
            lblImapPort.Text = "IMAP Port";
            // 
            // txtImapServer
            // 
            txtImapServer.Location = new Point(166, 157);
            txtImapServer.Name = "txtImapServer";
            txtImapServer.Size = new Size(455, 27);
            txtImapServer.TabIndex = 24;
            // 
            // lblImapServer
            // 
            lblImapServer.AutoSize = true;
            lblImapServer.Location = new Point(23, 164);
            lblImapServer.Name = "lblImapServer";
            lblImapServer.RightToLeft = RightToLeft.Yes;
            lblImapServer.Size = new Size(89, 20);
            lblImapServer.TabIndex = 23;
            lblImapServer.Text = "IMAP Server";
            // 
            // txtSmtpPort
            // 
            txtSmtpPort.Location = new Point(166, 124);
            txtSmtpPort.Name = "txtSmtpPort";
            txtSmtpPort.Size = new Size(455, 27);
            txtSmtpPort.TabIndex = 22;
            // 
            // lblsmtpPort
            // 
            lblsmtpPort.AutoSize = true;
            lblsmtpPort.Location = new Point(23, 131);
            lblsmtpPort.Name = "lblsmtpPort";
            lblsmtpPort.RightToLeft = RightToLeft.Yes;
            lblsmtpPort.Size = new Size(76, 20);
            lblsmtpPort.TabIndex = 21;
            lblsmtpPort.Text = "SMTP Port";
            // 
            // txtSMTPServer
            // 
            txtSMTPServer.Location = new Point(166, 91);
            txtSMTPServer.Name = "txtSMTPServer";
            txtSMTPServer.Size = new Size(455, 27);
            txtSMTPServer.TabIndex = 20;
            // 
            // lblSmtpHost
            // 
            lblSmtpHost.AutoSize = true;
            lblSmtpHost.Location = new Point(23, 98);
            lblSmtpHost.Name = "lblSmtpHost";
            lblSmtpHost.RightToLeft = RightToLeft.Yes;
            lblSmtpHost.Size = new Size(91, 20);
            lblSmtpHost.TabIndex = 19;
            lblSmtpHost.Text = "SMTP Server";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(166, 58);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(455, 27);
            txtPassword.TabIndex = 18;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(23, 65);
            lblPassword.Name = "lblPassword";
            lblPassword.RightToLeft = RightToLeft.Yes;
            lblPassword.Size = new Size(70, 20);
            lblPassword.TabIndex = 17;
            lblPassword.Text = "Password";
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(166, 25);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(455, 27);
            txtEmail.TabIndex = 16;
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(23, 32);
            lblEmail.Name = "lblEmail";
            lblEmail.RightToLeft = RightToLeft.Yes;
            lblEmail.Size = new Size(46, 20);
            lblEmail.TabIndex = 15;
            lblEmail.Text = "Email";
            // 
            // Add_Account
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(649, 356);
            Controls.Add(btnAdd);
            Controls.Add(ckImapSSL);
            Controls.Add(ckSmtpSSL);
            Controls.Add(txtImapPort);
            Controls.Add(lblImapPort);
            Controls.Add(txtImapServer);
            Controls.Add(lblImapServer);
            Controls.Add(txtSmtpPort);
            Controls.Add(lblsmtpPort);
            Controls.Add(txtSMTPServer);
            Controls.Add(lblSmtpHost);
            Controls.Add(txtPassword);
            Controls.Add(lblPassword);
            Controls.Add(txtEmail);
            Controls.Add(lblEmail);
            Name = "Add_Account";
            Text = "Add_Account";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnAdd;
        private CheckBox ckImapSSL;
        private CheckBox ckSmtpSSL;
        private TextBox txtImapPort;
        private Label lblImapPort;
        private TextBox txtImapServer;
        private Label lblImapServer;
        private TextBox txtSmtpPort;
        private Label lblsmtpPort;
        private TextBox txtSMTPServer;
        private Label lblSmtpHost;
        private TextBox txtPassword;
        private Label lblPassword;
        private TextBox txtEmail;
        private Label lblEmail;
    }
}