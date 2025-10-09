namespace TubeMailGorilla.Forms
{
    partial class Commentor
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
            btnClearResults = new Button();
            prgResult = new ProgressBar();
            lstResults = new ListBox();
            label2 = new Label();
            cboTemplate = new ComboBox();
            ckTestMode = new CheckBox();
            btnAddTest = new Button();
            cboABTest = new ComboBox();
            ckABTest = new CheckBox();
            ckHtmlMode = new CheckBox();
            lblBody = new Label();
            rtxtBody = new RichTextBox();
            txtSubject = new TextBox();
            lblSubject = new Label();
            btnSendEmail = new Button();
            btnRemoveTest = new Button();
            groupBox1 = new GroupBox();
            ckIMAPSSL = new CheckBox();
            ckSMTPSSL = new CheckBox();
            txtIMAPPort = new TextBox();
            lblIMAP = new Label();
            txtIMAPServer = new TextBox();
            lblIMAPServer = new Label();
            txtSMTPPort = new TextBox();
            lblSMTPPort = new Label();
            txtSMTPServer = new TextBox();
            lblSMTPServer = new Label();
            lblActiveEmail = new Label();
            cboEmailAddress = new ComboBox();
            txtPassword = new TextBox();
            lblPassword = new Label();
            txtEmail = new TextBox();
            lblEmail = new Label();
            grpSummary = new GroupBox();
            btnRefreshSummary = new Button();
            txtEmailReplies = new TextBox();
            txtEmailsForwarded = new TextBox();
            txtUniqueEmailsFound = new TextBox();
            lblEmailReplies = new Label();
            lblEmailsForward = new Label();
            lblUniqueEmailsFound = new Label();
            label1 = new Label();
            btnSearch = new Button();
            btnSearchBatch = new Button();
            ckHTTPMode = new CheckBox();
            ckValidateEmails = new CheckBox();
            txtKeyword = new TextBox();
            lblKeyword = new Label();
            txtMaxResult = new TextBox();
            lblMaxResult = new Label();
            groupBox1.SuspendLayout();
            grpSummary.SuspendLayout();
            SuspendLayout();
            // 
            // btnClearResults
            // 
            btnClearResults.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearResults.Location = new Point(1073, 165);
            btnClearResults.Margin = new Padding(3, 2, 3, 2);
            btnClearResults.Name = "btnClearResults";
            btnClearResults.Size = new Size(140, 84);
            btnClearResults.TabIndex = 92;
            btnClearResults.Text = "Clear Results";
            btnClearResults.UseVisualStyleBackColor = true;
            // 
            // prgResult
            // 
            prgResult.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            prgResult.Location = new Point(427, 253);
            prgResult.Margin = new Padding(3, 2, 3, 2);
            prgResult.Name = "prgResult";
            prgResult.Size = new Size(786, 30);
            prgResult.TabIndex = 90;
            prgResult.Value = 100;
            // 
            // lstResults
            // 
            lstResults.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lstResults.FormattingEnabled = true;
            lstResults.Location = new Point(427, 163);
            lstResults.Margin = new Padding(3, 2, 3, 2);
            lstResults.Name = "lstResults";
            lstResults.Size = new Size(640, 84);
            lstResults.TabIndex = 91;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(577, 577);
            label2.Name = "label2";
            label2.Size = new Size(115, 20);
            label2.TabIndex = 89;
            label2.Text = "Select Template";
            // 
            // cboTemplate
            // 
            cboTemplate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboTemplate.FormattingEnabled = true;
            cboTemplate.Location = new Point(699, 573);
            cboTemplate.Margin = new Padding(3, 2, 3, 2);
            cboTemplate.Name = "cboTemplate";
            cboTemplate.Size = new Size(295, 28);
            cboTemplate.TabIndex = 88;
            // 
            // ckTestMode
            // 
            ckTestMode.AutoSize = true;
            ckTestMode.Location = new Point(427, 589);
            ckTestMode.Margin = new Padding(3, 2, 3, 2);
            ckTestMode.Name = "ckTestMode";
            ckTestMode.Size = new Size(100, 24);
            ckTestMode.TabIndex = 87;
            ckTestMode.Text = "Test Mode";
            ckTestMode.UseVisualStyleBackColor = true;
            // 
            // btnAddTest
            // 
            btnAddTest.Anchor = AnchorStyles.Right;
            btnAddTest.Enabled = false;
            btnAddTest.Location = new Point(1010, 617);
            btnAddTest.Margin = new Padding(3, 2, 3, 2);
            btnAddTest.Name = "btnAddTest";
            btnAddTest.Size = new Size(101, 30);
            btnAddTest.TabIndex = 86;
            btnAddTest.Text = "Add";
            btnAddTest.UseVisualStyleBackColor = true;
            // 
            // cboABTest
            // 
            cboABTest.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            cboABTest.Enabled = false;
            cboABTest.FormattingEnabled = true;
            cboABTest.Location = new Point(578, 621);
            cboABTest.Margin = new Padding(3, 2, 3, 2);
            cboABTest.Name = "cboABTest";
            cboABTest.Size = new Size(416, 28);
            cboABTest.TabIndex = 85;
            // 
            // ckABTest
            // 
            ckABTest.AutoSize = true;
            ckABTest.Location = new Point(427, 617);
            ckABTest.Margin = new Padding(3, 2, 3, 2);
            ckABTest.Name = "ckABTest";
            ckABTest.Size = new Size(90, 24);
            ckABTest.TabIndex = 84;
            ckABTest.Text = "A/B Test ";
            ckABTest.UseVisualStyleBackColor = true;
            // 
            // ckHtmlMode
            // 
            ckHtmlMode.AutoSize = true;
            ckHtmlMode.Location = new Point(427, 565);
            ckHtmlMode.Margin = new Padding(3, 2, 3, 2);
            ckHtmlMode.Name = "ckHtmlMode";
            ckHtmlMode.Size = new Size(113, 24);
            ckHtmlMode.TabIndex = 83;
            ckHtmlMode.Text = "HTML Mode";
            ckHtmlMode.UseVisualStyleBackColor = true;
            // 
            // lblBody
            // 
            lblBody.AutoSize = true;
            lblBody.Location = new Point(427, 251);
            lblBody.Name = "lblBody";
            lblBody.Size = new Size(43, 20);
            lblBody.TabIndex = 82;
            lblBody.Text = "Body";
            // 
            // rtxtBody
            // 
            rtxtBody.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            rtxtBody.Location = new Point(427, 355);
            rtxtBody.Margin = new Padding(3, 2, 3, 2);
            rtxtBody.Name = "rtxtBody";
            rtxtBody.Size = new Size(786, 193);
            rtxtBody.TabIndex = 81;
            rtxtBody.Text = "";
            // 
            // txtSubject
            // 
            txtSubject.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSubject.Location = new Point(427, 315);
            txtSubject.Margin = new Padding(3, 2, 3, 2);
            txtSubject.Name = "txtSubject";
            txtSubject.Size = new Size(786, 27);
            txtSubject.TabIndex = 80;
            // 
            // lblSubject
            // 
            lblSubject.AutoSize = true;
            lblSubject.Location = new Point(427, 184);
            lblSubject.Name = "lblSubject";
            lblSubject.Size = new Size(58, 20);
            lblSubject.TabIndex = 79;
            lblSubject.Text = "Subject";
            // 
            // btnSendEmail
            // 
            btnSendEmail.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSendEmail.Location = new Point(1009, 658);
            btnSendEmail.Margin = new Padding(3, 2, 3, 2);
            btnSendEmail.Name = "btnSendEmail";
            btnSendEmail.Size = new Size(208, 50);
            btnSendEmail.TabIndex = 78;
            btnSendEmail.Text = "Send Email";
            btnSendEmail.UseVisualStyleBackColor = true;
            // 
            // btnRemoveTest
            // 
            btnRemoveTest.Anchor = AnchorStyles.Right;
            btnRemoveTest.Enabled = false;
            btnRemoveTest.Location = new Point(1117, 617);
            btnRemoveTest.Margin = new Padding(3, 2, 3, 2);
            btnRemoveTest.Name = "btnRemoveTest";
            btnRemoveTest.Size = new Size(101, 30);
            btnRemoveTest.TabIndex = 93;
            btnRemoveTest.Text = "Remove";
            btnRemoveTest.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(ckIMAPSSL);
            groupBox1.Controls.Add(ckSMTPSSL);
            groupBox1.Controls.Add(txtIMAPPort);
            groupBox1.Controls.Add(lblIMAP);
            groupBox1.Controls.Add(txtIMAPServer);
            groupBox1.Controls.Add(lblIMAPServer);
            groupBox1.Controls.Add(txtSMTPPort);
            groupBox1.Controls.Add(lblSMTPPort);
            groupBox1.Controls.Add(txtSMTPServer);
            groupBox1.Controls.Add(lblSMTPServer);
            groupBox1.Controls.Add(lblActiveEmail);
            groupBox1.Controls.Add(cboEmailAddress);
            groupBox1.Controls.Add(txtPassword);
            groupBox1.Controls.Add(lblPassword);
            groupBox1.Controls.Add(txtEmail);
            groupBox1.Controls.Add(lblEmail);
            groupBox1.Location = new Point(1, 152);
            groupBox1.Margin = new Padding(3, 2, 3, 2);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 2, 3, 2);
            groupBox1.Size = new Size(415, 308);
            groupBox1.TabIndex = 94;
            groupBox1.TabStop = false;
            groupBox1.Text = "Forward Email Address Details";
            // 
            // ckIMAPSSL
            // 
            ckIMAPSSL.AutoSize = true;
            ckIMAPSSL.Enabled = false;
            ckIMAPSSL.Location = new Point(158, 228);
            ckIMAPSSL.Margin = new Padding(3, 2, 3, 2);
            ckIMAPSSL.Name = "ckIMAPSSL";
            ckIMAPSSL.Size = new Size(93, 24);
            ckIMAPSSL.TabIndex = 16;
            ckIMAPSSL.Text = "IMAP SSL";
            ckIMAPSSL.UseVisualStyleBackColor = true;
            // 
            // ckSMTPSSL
            // 
            ckSMTPSSL.AutoSize = true;
            ckSMTPSSL.Enabled = false;
            ckSMTPSSL.Location = new Point(19, 228);
            ckSMTPSSL.Margin = new Padding(3, 2, 3, 2);
            ckSMTPSSL.Name = "ckSMTPSSL";
            ckSMTPSSL.Size = new Size(95, 24);
            ckSMTPSSL.TabIndex = 15;
            ckSMTPSSL.Text = "SMTP SSL";
            ckSMTPSSL.UseVisualStyleBackColor = true;
            // 
            // txtIMAPPort
            // 
            txtIMAPPort.Enabled = false;
            txtIMAPPort.Location = new Point(158, 190);
            txtIMAPPort.Margin = new Padding(3, 2, 3, 2);
            txtIMAPPort.Name = "txtIMAPPort";
            txtIMAPPort.Size = new Size(238, 27);
            txtIMAPPort.TabIndex = 14;
            // 
            // lblIMAP
            // 
            lblIMAP.AutoSize = true;
            lblIMAP.Location = new Point(19, 192);
            lblIMAP.Name = "lblIMAP";
            lblIMAP.Size = new Size(74, 20);
            lblIMAP.TabIndex = 13;
            lblIMAP.Text = "IMAP Port";
            // 
            // txtIMAPServer
            // 
            txtIMAPServer.Enabled = false;
            txtIMAPServer.Location = new Point(158, 158);
            txtIMAPServer.Margin = new Padding(3, 2, 3, 2);
            txtIMAPServer.Name = "txtIMAPServer";
            txtIMAPServer.Size = new Size(238, 27);
            txtIMAPServer.TabIndex = 12;
            // 
            // lblIMAPServer
            // 
            lblIMAPServer.AutoSize = true;
            lblIMAPServer.Location = new Point(19, 160);
            lblIMAPServer.Name = "lblIMAPServer";
            lblIMAPServer.Size = new Size(89, 20);
            lblIMAPServer.TabIndex = 11;
            lblIMAPServer.Text = "IMAP Server";
            // 
            // txtSMTPPort
            // 
            txtSMTPPort.Enabled = false;
            txtSMTPPort.Location = new Point(158, 126);
            txtSMTPPort.Margin = new Padding(3, 2, 3, 2);
            txtSMTPPort.Name = "txtSMTPPort";
            txtSMTPPort.Size = new Size(238, 27);
            txtSMTPPort.TabIndex = 10;
            // 
            // lblSMTPPort
            // 
            lblSMTPPort.AutoSize = true;
            lblSMTPPort.Location = new Point(19, 126);
            lblSMTPPort.Name = "lblSMTPPort";
            lblSMTPPort.Size = new Size(76, 20);
            lblSMTPPort.TabIndex = 9;
            lblSMTPPort.Text = "SMTP Port";
            // 
            // txtSMTPServer
            // 
            txtSMTPServer.Enabled = false;
            txtSMTPServer.Location = new Point(158, 92);
            txtSMTPServer.Margin = new Padding(3, 2, 3, 2);
            txtSMTPServer.Name = "txtSMTPServer";
            txtSMTPServer.Size = new Size(238, 27);
            txtSMTPServer.TabIndex = 8;
            // 
            // lblSMTPServer
            // 
            lblSMTPServer.AutoSize = true;
            lblSMTPServer.Location = new Point(19, 92);
            lblSMTPServer.Name = "lblSMTPServer";
            lblSMTPServer.Size = new Size(91, 20);
            lblSMTPServer.TabIndex = 7;
            lblSMTPServer.Text = "SMTP Server";
            // 
            // lblActiveEmail
            // 
            lblActiveEmail.AutoSize = true;
            lblActiveEmail.Location = new Point(17, 262);
            lblActiveEmail.Name = "lblActiveEmail";
            lblActiveEmail.Size = new Size(108, 20);
            lblActiveEmail.TabIndex = 6;
            lblActiveEmail.Text = "Active Account";
            // 
            // cboEmailAddress
            // 
            cboEmailAddress.FormattingEnabled = true;
            cboEmailAddress.Location = new Point(158, 262);
            cboEmailAddress.Margin = new Padding(3, 2, 3, 2);
            cboEmailAddress.Name = "cboEmailAddress";
            cboEmailAddress.Size = new Size(236, 28);
            cboEmailAddress.TabIndex = 5;
            // 
            // txtPassword
            // 
            txtPassword.Enabled = false;
            txtPassword.Location = new Point(158, 60);
            txtPassword.Margin = new Padding(3, 2, 3, 2);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(238, 27);
            txtPassword.TabIndex = 3;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(19, 60);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(70, 20);
            lblPassword.TabIndex = 2;
            lblPassword.Text = "Password";
            // 
            // txtEmail
            // 
            txtEmail.Enabled = false;
            txtEmail.Location = new Point(158, 26);
            txtEmail.Margin = new Padding(3, 2, 3, 2);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(238, 27);
            txtEmail.TabIndex = 1;
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(19, 28);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(50, 20);
            lblEmail.TabIndex = 0;
            lblEmail.Text = "Email ";
            // 
            // grpSummary
            // 
            grpSummary.Controls.Add(btnRefreshSummary);
            grpSummary.Controls.Add(txtEmailReplies);
            grpSummary.Controls.Add(txtEmailsForwarded);
            grpSummary.Controls.Add(txtUniqueEmailsFound);
            grpSummary.Controls.Add(lblEmailReplies);
            grpSummary.Controls.Add(lblEmailsForward);
            grpSummary.Controls.Add(lblUniqueEmailsFound);
            grpSummary.Location = new Point(2, 463);
            grpSummary.Margin = new Padding(3, 2, 3, 2);
            grpSummary.Name = "grpSummary";
            grpSummary.Padding = new Padding(3, 2, 3, 2);
            grpSummary.Size = new Size(407, 193);
            grpSummary.TabIndex = 95;
            grpSummary.TabStop = false;
            grpSummary.Text = "Summary";
            // 
            // btnRefreshSummary
            // 
            btnRefreshSummary.Location = new Point(301, 149);
            btnRefreshSummary.Margin = new Padding(3, 2, 3, 2);
            btnRefreshSummary.Name = "btnRefreshSummary";
            btnRefreshSummary.Size = new Size(94, 30);
            btnRefreshSummary.TabIndex = 5;
            btnRefreshSummary.Text = "Refresh";
            btnRefreshSummary.UseVisualStyleBackColor = true;
            // 
            // txtEmailReplies
            // 
            txtEmailReplies.Location = new Point(214, 109);
            txtEmailReplies.Margin = new Padding(3, 2, 3, 2);
            txtEmailReplies.Name = "txtEmailReplies";
            txtEmailReplies.Size = new Size(180, 27);
            txtEmailReplies.TabIndex = 4;
            // 
            // txtEmailsForwarded
            // 
            txtEmailsForwarded.Location = new Point(214, 76);
            txtEmailsForwarded.Margin = new Padding(3, 2, 3, 2);
            txtEmailsForwarded.Name = "txtEmailsForwarded";
            txtEmailsForwarded.Size = new Size(180, 27);
            txtEmailsForwarded.TabIndex = 3;
            // 
            // txtUniqueEmailsFound
            // 
            txtUniqueEmailsFound.Location = new Point(214, 45);
            txtUniqueEmailsFound.Margin = new Padding(3, 2, 3, 2);
            txtUniqueEmailsFound.Name = "txtUniqueEmailsFound";
            txtUniqueEmailsFound.Size = new Size(180, 27);
            txtUniqueEmailsFound.TabIndex = 2;
            // 
            // lblEmailReplies
            // 
            lblEmailReplies.AutoSize = true;
            lblEmailReplies.Location = new Point(22, 110);
            lblEmailReplies.Name = "lblEmailReplies";
            lblEmailReplies.Size = new Size(98, 20);
            lblEmailReplies.TabIndex = 1;
            lblEmailReplies.Text = "Email Replies";
            // 
            // lblEmailsForward
            // 
            lblEmailsForward.AutoSize = true;
            lblEmailsForward.Location = new Point(22, 80);
            lblEmailsForward.Name = "lblEmailsForward";
            lblEmailsForward.Size = new Size(127, 20);
            lblEmailsForward.TabIndex = 0;
            lblEmailsForward.Text = "Emails Forwarded";
            // 
            // lblUniqueEmailsFound
            // 
            lblUniqueEmailsFound.AutoSize = true;
            lblUniqueEmailsFound.Location = new Point(22, 51);
            lblUniqueEmailsFound.Name = "lblUniqueEmailsFound";
            lblUniqueEmailsFound.Size = new Size(148, 20);
            lblUniqueEmailsFound.TabIndex = 0;
            lblUniqueEmailsFound.Text = "Unique Emails Found";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(427, 293);
            label1.Name = "label1";
            label1.Size = new Size(58, 20);
            label1.TabIndex = 96;
            label1.Text = "Subject";
            // 
            // btnSearch
            // 
            btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSearch.Location = new Point(1072, 21);
            btnSearch.Margin = new Padding(3, 2, 3, 2);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(140, 61);
            btnSearch.TabIndex = 99;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnSearchBatch
            // 
            btnSearchBatch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSearchBatch.Location = new Point(1072, 86);
            btnSearchBatch.Margin = new Padding(3, 2, 3, 2);
            btnSearchBatch.Name = "btnSearchBatch";
            btnSearchBatch.Size = new Size(140, 61);
            btnSearchBatch.TabIndex = 100;
            btnSearchBatch.Text = "Batch";
            btnSearchBatch.UseVisualStyleBackColor = true;
            btnSearchBatch.Click += btnSearchBatch_Click;
            // 
            // ckHTTPMode
            // 
            ckHTTPMode.AutoSize = true;
            ckHTTPMode.Location = new Point(886, 68);
            ckHTTPMode.Margin = new Padding(3, 2, 3, 2);
            ckHTTPMode.Name = "ckHTTPMode";
            ckHTTPMode.Size = new Size(109, 24);
            ckHTTPMode.TabIndex = 104;
            ckHTTPMode.Text = "HTTP Mode";
            ckHTTPMode.UseVisualStyleBackColor = true;
            // 
            // ckValidateEmails
            // 
            ckValidateEmails.AutoSize = true;
            ckValidateEmails.Location = new Point(886, 40);
            ckValidateEmails.Margin = new Padding(3, 2, 3, 2);
            ckValidateEmails.Name = "ckValidateEmails";
            ckValidateEmails.Size = new Size(132, 24);
            ckValidateEmails.TabIndex = 103;
            ckValidateEmails.Text = "Validate Emails";
            ckValidateEmails.UseVisualStyleBackColor = true;
            // 
            // txtKeyword
            // 
            txtKeyword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtKeyword.Location = new Point(216, 38);
            txtKeyword.Margin = new Padding(3, 2, 3, 2);
            txtKeyword.Name = "txtKeyword";
            txtKeyword.Size = new Size(654, 27);
            txtKeyword.TabIndex = 106;
            // 
            // lblKeyword
            // 
            lblKeyword.AutoSize = true;
            lblKeyword.Location = new Point(20, 41);
            lblKeyword.Name = "lblKeyword";
            lblKeyword.Size = new Size(114, 20);
            lblKeyword.TabIndex = 105;
            lblKeyword.Text = "YouTube Search";
            // 
            // txtMaxResult
            // 
            txtMaxResult.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMaxResult.Location = new Point(216, 72);
            txtMaxResult.Margin = new Padding(3, 2, 3, 2);
            txtMaxResult.Name = "txtMaxResult";
            txtMaxResult.Size = new Size(654, 27);
            txtMaxResult.TabIndex = 108;
            // 
            // lblMaxResult
            // 
            lblMaxResult.AutoSize = true;
            lblMaxResult.Location = new Point(20, 75);
            lblMaxResult.Name = "lblMaxResult";
            lblMaxResult.Size = new Size(81, 20);
            lblMaxResult.TabIndex = 107;
            lblMaxResult.Text = "Max Result";
            // 
            // Commentor
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1224, 719);
            Controls.Add(txtMaxResult);
            Controls.Add(lblMaxResult);
            Controls.Add(txtKeyword);
            Controls.Add(lblKeyword);
            Controls.Add(ckHTTPMode);
            Controls.Add(ckValidateEmails);
            Controls.Add(btnSearchBatch);
            Controls.Add(btnSearch);
            Controls.Add(label1);
            Controls.Add(groupBox1);
            Controls.Add(grpSummary);
            Controls.Add(btnRemoveTest);
            Controls.Add(btnClearResults);
            Controls.Add(prgResult);
            Controls.Add(lstResults);
            Controls.Add(label2);
            Controls.Add(cboTemplate);
            Controls.Add(ckTestMode);
            Controls.Add(btnAddTest);
            Controls.Add(cboABTest);
            Controls.Add(ckABTest);
            Controls.Add(ckHtmlMode);
            Controls.Add(lblBody);
            Controls.Add(rtxtBody);
            Controls.Add(txtSubject);
            Controls.Add(lblSubject);
            Controls.Add(btnSendEmail);
            Name = "Commentor";
            Text = "Commentor";
            Load += Commentor_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            grpSummary.ResumeLayout(false);
            grpSummary.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnClearResults;
        private ProgressBar prgResult;
        private ListBox lstResults;
        private Label label2;
        private ComboBox cboTemplate;
        private CheckBox ckTestMode;
        private Button btnAddTest;
        private ComboBox cboABTest;
        private CheckBox ckABTest;
        private CheckBox ckHtmlMode;
        private Label lblBody;
        private RichTextBox rtxtBody;
        private TextBox txtSubject;
        private Label lblSubject;
        private Button btnSendEmail;
        private Button btnRemoveTest;
        private GroupBox groupBox1;
        private CheckBox ckIMAPSSL;
        private CheckBox ckSMTPSSL;
        private TextBox txtIMAPPort;
        private Label lblIMAP;
        private TextBox txtIMAPServer;
        private Label lblIMAPServer;
        private TextBox txtSMTPPort;
        private Label lblSMTPPort;
        private TextBox txtSMTPServer;
        private Label lblSMTPServer;
        private Label lblActiveEmail;
        private ComboBox cboEmailAddress;
        private TextBox txtPassword;
        private Label lblPassword;
        private TextBox txtEmail;
        private Label lblEmail;
        private GroupBox grpSummary;
        private Button btnRefreshSummary;
        private TextBox txtEmailReplies;
        private TextBox txtEmailsForwarded;
        private TextBox txtUniqueEmailsFound;
        private Label lblEmailReplies;
        private Label lblEmailsForward;
        private Label lblUniqueEmailsFound;
        private Label label1;
        private Button btnSearch;
        private Button btnSearchBatch;
        private CheckBox ckHTTPMode;
        private CheckBox ckValidateEmails;
        private TextBox txtKeyword;
        private Label lblKeyword;
        private TextBox txtMaxResult;
        private Label lblMaxResult;
    }
}