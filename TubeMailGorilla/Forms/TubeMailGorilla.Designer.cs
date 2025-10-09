namespace TubeMailGorilla.Forms
{
    partial class TubeMailGorilla
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
            btnSearch = new Button();
            txtLikeCount = new TextBox();
            lblLikeCount = new Label();
            txtViewCount = new TextBox();
            lblViewCount = new Label();
            groupBox2 = new GroupBox();
            ckInsights = new CheckBox();
            ckHTTPMode = new CheckBox();
            ckValidateEmails = new CheckBox();
            ckEmailUserName = new CheckBox();
            ckGmailAccountOnly = new CheckBox();
            ckAccelerateMode = new CheckBox();
            btnBatchSearch = new Button();
            txtNoVideoInChannel = new TextBox();
            lblNoVideosInChannels = new Label();
            txtPageViewLimit = new TextBox();
            label1 = new Label();
            txtKeyword = new TextBox();
            lblKeyword = new Label();
            btnClearResults = new Button();
            prgResult = new ProgressBar();
            lstResults = new ListBox();
            label2 = new Label();
            cboTemplate = new ComboBox();
            ckTestMode = new CheckBox();
            btnRemoveTest = new Button();
            btnAddTest = new Button();
            cboABTest = new ComboBox();
            ckABTest = new CheckBox();
            ckHtmlMode = new CheckBox();
            lblBody = new Label();
            rtxtBody = new RichTextBox();
            txtSubject = new TextBox();
            lblSubject = new Label();
            btnCommentor = new Button();
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
            btnFollowUp = new Button();
            grpSummary = new GroupBox();
            btnRefreshSummary = new Button();
            txtEmailReplies = new TextBox();
            txtEmailsForwarded = new TextBox();
            txtUniqueEmailsFound = new TextBox();
            lblEmailReplies = new Label();
            lblEmailsForward = new Label();
            lblUniqueEmailsFound = new Label();
            btnSendEmail = new Button();
            btnDataControls = new Button();
            menuStrip1 = new MenuStrip();
            dataControlsToolStripMenuItem = new ToolStripMenuItem();
            dataControlsToolStripMenuItem1 = new ToolStripMenuItem();
            blockedEmailAddressToolStripMenuItem = new ToolStripMenuItem();
            aIAssistantToolStripMenuItem = new ToolStripMenuItem();
            iceBreakersToolStripMenuItem = new ToolStripMenuItem();
            accountsToolStripMenuItem = new ToolStripMenuItem();
            addNewAccountToolStripMenuItem = new ToolStripMenuItem();
            templatesToolStripMenuItem = new ToolStripMenuItem();
            proxiesToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            grpSummary.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btnSearch
            // 
            btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSearch.Location = new Point(1071, 92);
            btnSearch.Margin = new Padding(3, 2, 3, 2);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(173, 54);
            btnSearch.TabIndex = 94;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // txtLikeCount
            // 
            txtLikeCount.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtLikeCount.Location = new Point(198, 192);
            txtLikeCount.Margin = new Padding(3, 2, 3, 2);
            txtLikeCount.Name = "txtLikeCount";
            txtLikeCount.Size = new Size(669, 27);
            txtLikeCount.TabIndex = 89;
            // 
            // lblLikeCount
            // 
            lblLikeCount.AutoSize = true;
            lblLikeCount.Location = new Point(33, 198);
            lblLikeCount.Name = "lblLikeCount";
            lblLikeCount.Size = new Size(127, 20);
            lblLikeCount.TabIndex = 88;
            lblLikeCount.Text = "Video Likes Count";
            // 
            // txtViewCount
            // 
            txtViewCount.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtViewCount.Location = new Point(198, 157);
            txtViewCount.Margin = new Padding(3, 2, 3, 2);
            txtViewCount.Name = "txtViewCount";
            txtViewCount.Size = new Size(669, 27);
            txtViewCount.TabIndex = 87;
            // 
            // lblViewCount
            // 
            lblViewCount.AutoSize = true;
            lblViewCount.Location = new Point(33, 164);
            lblViewCount.Name = "lblViewCount";
            lblViewCount.Size = new Size(127, 20);
            lblViewCount.TabIndex = 86;
            lblViewCount.Text = "Video View Count";
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            groupBox2.Controls.Add(ckInsights);
            groupBox2.Controls.Add(ckHTTPMode);
            groupBox2.Controls.Add(ckValidateEmails);
            groupBox2.Controls.Add(ckEmailUserName);
            groupBox2.Controls.Add(ckGmailAccountOnly);
            groupBox2.Controls.Add(ckAccelerateMode);
            groupBox2.Location = new Point(873, 41);
            groupBox2.Margin = new Padding(3, 2, 3, 2);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(3, 2, 3, 2);
            groupBox2.Size = new Size(192, 206);
            groupBox2.TabIndex = 85;
            groupBox2.TabStop = false;
            groupBox2.Text = "Options";
            // 
            // ckInsights
            // 
            ckInsights.AutoSize = true;
            ckInsights.Location = new Point(17, 171);
            ckInsights.Margin = new Padding(3, 2, 3, 2);
            ckInsights.Name = "ckInsights";
            ckInsights.Size = new Size(81, 24);
            ckInsights.TabIndex = 13;
            ckInsights.Text = "Insights";
            ckInsights.UseVisualStyleBackColor = true;
            // 
            // ckHTTPMode
            // 
            ckHTTPMode.AutoSize = true;
            ckHTTPMode.Location = new Point(17, 141);
            ckHTTPMode.Margin = new Padding(3, 2, 3, 2);
            ckHTTPMode.Name = "ckHTTPMode";
            ckHTTPMode.Size = new Size(109, 24);
            ckHTTPMode.TabIndex = 12;
            ckHTTPMode.Text = "HTTP Mode";
            ckHTTPMode.UseVisualStyleBackColor = true;
            // 
            // ckValidateEmails
            // 
            ckValidateEmails.AutoSize = true;
            ckValidateEmails.Location = new Point(17, 81);
            ckValidateEmails.Margin = new Padding(3, 2, 3, 2);
            ckValidateEmails.Name = "ckValidateEmails";
            ckValidateEmails.Size = new Size(132, 24);
            ckValidateEmails.TabIndex = 11;
            ckValidateEmails.Text = "Validate Emails";
            ckValidateEmails.UseVisualStyleBackColor = true;
            // 
            // ckEmailUserName
            // 
            ckEmailUserName.AutoSize = true;
            ckEmailUserName.Location = new Point(17, 21);
            ckEmailUserName.Margin = new Padding(3, 2, 3, 2);
            ckEmailUserName.Name = "ckEmailUserName";
            ckEmailUserName.Size = new Size(166, 24);
            ckEmailUserName.TabIndex = 7;
            ckEmailUserName.Text = "Use Email Username";
            ckEmailUserName.UseVisualStyleBackColor = true;
            // 
            // ckGmailAccountOnly
            // 
            ckGmailAccountOnly.AutoSize = true;
            ckGmailAccountOnly.Location = new Point(17, 51);
            ckGmailAccountOnly.Margin = new Padding(3, 2, 3, 2);
            ckGmailAccountOnly.Name = "ckGmailAccountOnly";
            ckGmailAccountOnly.Size = new Size(162, 24);
            ckGmailAccountOnly.TabIndex = 8;
            ckGmailAccountOnly.Text = "Gmail Account Only";
            ckGmailAccountOnly.UseVisualStyleBackColor = true;
            // 
            // ckAccelerateMode
            // 
            ckAccelerateMode.AutoSize = true;
            ckAccelerateMode.Location = new Point(17, 111);
            ckAccelerateMode.Margin = new Padding(3, 2, 3, 2);
            ckAccelerateMode.Name = "ckAccelerateMode";
            ckAccelerateMode.Size = new Size(144, 24);
            ckAccelerateMode.TabIndex = 9;
            ckAccelerateMode.Text = "Accelerate Mode";
            ckAccelerateMode.UseVisualStyleBackColor = true;
            // 
            // btnBatchSearch
            // 
            btnBatchSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBatchSearch.Location = new Point(1071, 150);
            btnBatchSearch.Margin = new Padding(3, 2, 3, 2);
            btnBatchSearch.Name = "btnBatchSearch";
            btnBatchSearch.Size = new Size(173, 54);
            btnBatchSearch.TabIndex = 84;
            btnBatchSearch.Text = "Batch Search";
            btnBatchSearch.UseVisualStyleBackColor = true;
            btnBatchSearch.Click += btnBatchSearch_Click;
            // 
            // txtNoVideoInChannel
            // 
            txtNoVideoInChannel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtNoVideoInChannel.Location = new Point(198, 125);
            txtNoVideoInChannel.Margin = new Padding(3, 2, 3, 2);
            txtNoVideoInChannel.Name = "txtNoVideoInChannel";
            txtNoVideoInChannel.Size = new Size(669, 27);
            txtNoVideoInChannel.TabIndex = 83;
            // 
            // lblNoVideosInChannels
            // 
            lblNoVideosInChannels.AutoSize = true;
            lblNoVideosInChannels.Location = new Point(33, 130);
            lblNoVideosInChannels.Name = "lblNoVideosInChannels";
            lblNoVideosInChannels.Size = new Size(140, 20);
            lblNoVideosInChannels.TabIndex = 82;
            lblNoVideosInChannels.Text = "No. Videos/Channel";
            // 
            // txtPageViewLimit
            // 
            txtPageViewLimit.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPageViewLimit.Location = new Point(198, 93);
            txtPageViewLimit.Margin = new Padding(3, 2, 3, 2);
            txtPageViewLimit.Name = "txtPageViewLimit";
            txtPageViewLimit.Size = new Size(669, 27);
            txtPageViewLimit.TabIndex = 81;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(32, 100);
            label1.Name = "label1";
            label1.Size = new Size(90, 20);
            label1.TabIndex = 80;
            label1.Text = "Search Limit";
            // 
            // txtKeyword
            // 
            txtKeyword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtKeyword.Location = new Point(198, 60);
            txtKeyword.Margin = new Padding(3, 2, 3, 2);
            txtKeyword.Name = "txtKeyword";
            txtKeyword.Size = new Size(668, 27);
            txtKeyword.TabIndex = 79;
            // 
            // lblKeyword
            // 
            lblKeyword.AutoSize = true;
            lblKeyword.Location = new Point(32, 61);
            lblKeyword.Name = "lblKeyword";
            lblKeyword.Size = new Size(67, 20);
            lblKeyword.TabIndex = 78;
            lblKeyword.Text = "Keyword";
            // 
            // btnClearResults
            // 
            btnClearResults.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearResults.Location = new Point(1092, 255);
            btnClearResults.Margin = new Padding(3, 2, 3, 2);
            btnClearResults.Name = "btnClearResults";
            btnClearResults.Size = new Size(152, 84);
            btnClearResults.TabIndex = 77;
            btnClearResults.Text = "Clear Results";
            btnClearResults.UseVisualStyleBackColor = true;
            btnClearResults.Click += btnClearResults_Click;
            // 
            // prgResult
            // 
            prgResult.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            prgResult.Location = new Point(453, 345);
            prgResult.Margin = new Padding(3, 2, 3, 2);
            prgResult.Name = "prgResult";
            prgResult.Size = new Size(794, 30);
            prgResult.TabIndex = 75;
            prgResult.Value = 100;
            // 
            // lstResults
            // 
            lstResults.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lstResults.FormattingEnabled = true;
            lstResults.Location = new Point(453, 255);
            lstResults.Margin = new Padding(3, 2, 3, 2);
            lstResults.Name = "lstResults";
            lstResults.Size = new Size(632, 84);
            lstResults.TabIndex = 76;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(598, 673);
            label2.Name = "label2";
            label2.Size = new Size(115, 20);
            label2.TabIndex = 74;
            label2.Text = "Select Template";
            // 
            // cboTemplate
            // 
            cboTemplate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboTemplate.FormattingEnabled = true;
            cboTemplate.Location = new Point(720, 669);
            cboTemplate.Margin = new Padding(3, 2, 3, 2);
            cboTemplate.Name = "cboTemplate";
            cboTemplate.Size = new Size(295, 28);
            cboTemplate.TabIndex = 73;
            cboTemplate.SelectedIndexChanged += cboTemplate_SelectedIndexChanged;
            // 
            // ckTestMode
            // 
            ckTestMode.AutoSize = true;
            ckTestMode.Location = new Point(453, 693);
            ckTestMode.Margin = new Padding(3, 2, 3, 2);
            ckTestMode.Name = "ckTestMode";
            ckTestMode.Size = new Size(100, 24);
            ckTestMode.TabIndex = 72;
            ckTestMode.Text = "Test Mode";
            ckTestMode.UseVisualStyleBackColor = true;
            // 
            // btnRemoveTest
            // 
            btnRemoveTest.Anchor = AnchorStyles.Right;
            btnRemoveTest.Enabled = false;
            btnRemoveTest.Location = new Point(1138, 719);
            btnRemoveTest.Margin = new Padding(3, 2, 3, 2);
            btnRemoveTest.Name = "btnRemoveTest";
            btnRemoveTest.Size = new Size(101, 30);
            btnRemoveTest.TabIndex = 71;
            btnRemoveTest.Text = "Remove";
            btnRemoveTest.UseVisualStyleBackColor = true;
            btnRemoveTest.Click += btnRemoveTest_Click;
            // 
            // btnAddTest
            // 
            btnAddTest.Anchor = AnchorStyles.Right;
            btnAddTest.Enabled = false;
            btnAddTest.Location = new Point(1031, 719);
            btnAddTest.Margin = new Padding(3, 2, 3, 2);
            btnAddTest.Name = "btnAddTest";
            btnAddTest.Size = new Size(101, 30);
            btnAddTest.TabIndex = 70;
            btnAddTest.Text = "Add";
            btnAddTest.UseVisualStyleBackColor = true;
            btnAddTest.Click += btnAddTest_Click;
            // 
            // cboABTest
            // 
            cboABTest.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            cboABTest.Enabled = false;
            cboABTest.FormattingEnabled = true;
            cboABTest.Location = new Point(599, 723);
            cboABTest.Margin = new Padding(3, 2, 3, 2);
            cboABTest.Name = "cboABTest";
            cboABTest.Size = new Size(416, 28);
            cboABTest.TabIndex = 69;
            // 
            // ckABTest
            // 
            ckABTest.AutoSize = true;
            ckABTest.Location = new Point(453, 721);
            ckABTest.Margin = new Padding(3, 2, 3, 2);
            ckABTest.Name = "ckABTest";
            ckABTest.Size = new Size(90, 24);
            ckABTest.TabIndex = 68;
            ckABTest.Text = "A/B Test ";
            ckABTest.UseVisualStyleBackColor = true;
            ckABTest.CheckedChanged += ckABTest_CheckedChanged;
            // 
            // ckHtmlMode
            // 
            ckHtmlMode.AutoSize = true;
            ckHtmlMode.Location = new Point(453, 669);
            ckHtmlMode.Margin = new Padding(3, 2, 3, 2);
            ckHtmlMode.Name = "ckHtmlMode";
            ckHtmlMode.Size = new Size(113, 24);
            ckHtmlMode.TabIndex = 67;
            ckHtmlMode.Text = "HTML Mode";
            ckHtmlMode.UseVisualStyleBackColor = true;
            // 
            // lblBody
            // 
            lblBody.AutoSize = true;
            lblBody.Location = new Point(453, 452);
            lblBody.Name = "lblBody";
            lblBody.Size = new Size(43, 20);
            lblBody.TabIndex = 66;
            lblBody.Text = "Body";
            // 
            // rtxtBody
            // 
            rtxtBody.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            rtxtBody.Location = new Point(453, 487);
            rtxtBody.Margin = new Padding(3, 2, 3, 2);
            rtxtBody.Name = "rtxtBody";
            rtxtBody.Size = new Size(786, 169);
            rtxtBody.TabIndex = 65;
            rtxtBody.Text = "";
            // 
            // txtSubject
            // 
            txtSubject.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSubject.Location = new Point(453, 407);
            txtSubject.Margin = new Padding(3, 2, 3, 2);
            txtSubject.Name = "txtSubject";
            txtSubject.Size = new Size(758, 27);
            txtSubject.TabIndex = 64;
            // 
            // lblSubject
            // 
            lblSubject.AutoSize = true;
            lblSubject.Location = new Point(453, 385);
            lblSubject.Name = "lblSubject";
            lblSubject.Size = new Size(58, 20);
            lblSubject.TabIndex = 63;
            lblSubject.Text = "Subject";
            // 
            // btnCommentor
            // 
            btnCommentor.Location = new Point(297, 759);
            btnCommentor.Margin = new Padding(3, 2, 3, 2);
            btnCommentor.Name = "btnCommentor";
            btnCommentor.Size = new Size(123, 50);
            btnCommentor.TabIndex = 62;
            btnCommentor.Text = "Commentor";
            btnCommentor.UseVisualStyleBackColor = true;
            btnCommentor.Click += btnCommentor_Click;
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
            groupBox1.Location = new Point(12, 245);
            groupBox1.Margin = new Padding(3, 2, 3, 2);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 2, 3, 2);
            groupBox1.Size = new Size(415, 308);
            groupBox1.TabIndex = 56;
            groupBox1.TabStop = false;
            groupBox1.Text = "Forward Email Address Details";
            // 
            // ckIMAPSSL
            // 
            ckIMAPSSL.AutoSize = true;
            ckIMAPSSL.Enabled = false;
            ckIMAPSSL.Location = new Point(158, 229);
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
            ckSMTPSSL.Location = new Point(19, 229);
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
            lblIMAP.Location = new Point(19, 193);
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
            lblIMAPServer.Location = new Point(19, 161);
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
            lblSMTPPort.Location = new Point(19, 127);
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
            lblSMTPServer.Location = new Point(19, 93);
            lblSMTPServer.Name = "lblSMTPServer";
            lblSMTPServer.Size = new Size(91, 20);
            lblSMTPServer.TabIndex = 7;
            lblSMTPServer.Text = "SMTP Server";
            // 
            // lblActiveEmail
            // 
            lblActiveEmail.AutoSize = true;
            lblActiveEmail.Location = new Point(17, 263);
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
            cboEmailAddress.SelectedIndexChanged += cboEmailAddress_SelectedIndexChanged;
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
            lblPassword.Location = new Point(19, 61);
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
            lblEmail.Location = new Point(19, 29);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(50, 20);
            lblEmail.TabIndex = 0;
            lblEmail.Text = "Email ";
            // 
            // btnFollowUp
            // 
            btnFollowUp.Location = new Point(150, 760);
            btnFollowUp.Margin = new Padding(3, 2, 3, 2);
            btnFollowUp.Name = "btnFollowUp";
            btnFollowUp.Size = new Size(123, 50);
            btnFollowUp.TabIndex = 61;
            btnFollowUp.Text = "Follow Up's";
            btnFollowUp.UseVisualStyleBackColor = true;
            btnFollowUp.Click += btnFollowUp_Click;
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
            grpSummary.Location = new Point(13, 556);
            grpSummary.Margin = new Padding(3, 2, 3, 2);
            grpSummary.Name = "grpSummary";
            grpSummary.Padding = new Padding(3, 2, 3, 2);
            grpSummary.Size = new Size(407, 193);
            grpSummary.TabIndex = 60;
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
            btnRefreshSummary.Click += btnRefreshSummary_Click;
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
            lblEmailReplies.Location = new Point(22, 111);
            lblEmailReplies.Name = "lblEmailReplies";
            lblEmailReplies.Size = new Size(98, 20);
            lblEmailReplies.TabIndex = 1;
            lblEmailReplies.Text = "Email Replies";
            // 
            // lblEmailsForward
            // 
            lblEmailsForward.AutoSize = true;
            lblEmailsForward.Location = new Point(22, 81);
            lblEmailsForward.Name = "lblEmailsForward";
            lblEmailsForward.Size = new Size(127, 20);
            lblEmailsForward.TabIndex = 0;
            lblEmailsForward.Text = "Emails Forwarded";
            // 
            // lblUniqueEmailsFound
            // 
            lblUniqueEmailsFound.AutoSize = true;
            lblUniqueEmailsFound.Location = new Point(22, 52);
            lblUniqueEmailsFound.Name = "lblUniqueEmailsFound";
            lblUniqueEmailsFound.Size = new Size(148, 20);
            lblUniqueEmailsFound.TabIndex = 0;
            lblUniqueEmailsFound.Text = "Unique Emails Found";
            // 
            // btnSendEmail
            // 
            btnSendEmail.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSendEmail.Location = new Point(1031, 759);
            btnSendEmail.Margin = new Padding(3, 2, 3, 2);
            btnSendEmail.Name = "btnSendEmail";
            btnSendEmail.Size = new Size(208, 50);
            btnSendEmail.TabIndex = 58;
            btnSendEmail.Text = "Send Email";
            btnSendEmail.UseVisualStyleBackColor = true;
            btnSendEmail.Click += btnSendEmail_Click;
            // 
            // btnDataControls
            // 
            btnDataControls.Location = new Point(10, 760);
            btnDataControls.Margin = new Padding(3, 2, 3, 2);
            btnDataControls.Name = "btnDataControls";
            btnDataControls.Size = new Size(123, 50);
            btnDataControls.TabIndex = 57;
            btnDataControls.Text = "Data Controls";
            btnDataControls.UseVisualStyleBackColor = true;
            btnDataControls.Click += btnDataControls_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { dataControlsToolStripMenuItem, accountsToolStripMenuItem, templatesToolStripMenuItem, proxiesToolStripMenuItem, settingsToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1256, 28);
            menuStrip1.TabIndex = 59;
            menuStrip1.Text = "menuStrip1";
            // 
            // dataControlsToolStripMenuItem
            // 
            dataControlsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { dataControlsToolStripMenuItem1, blockedEmailAddressToolStripMenuItem, aIAssistantToolStripMenuItem, iceBreakersToolStripMenuItem });
            dataControlsToolStripMenuItem.Name = "dataControlsToolStripMenuItem";
            dataControlsToolStripMenuItem.Size = new Size(114, 24);
            dataControlsToolStripMenuItem.Text = "Data Controls";
            // 
            // dataControlsToolStripMenuItem1
            // 
            dataControlsToolStripMenuItem1.Name = "dataControlsToolStripMenuItem1";
            dataControlsToolStripMenuItem1.Size = new Size(257, 26);
            dataControlsToolStripMenuItem1.Text = "Data Controls";
            dataControlsToolStripMenuItem1.Click += dataControlsToolStripMenuItem1_Click;
            // 
            // blockedEmailAddressToolStripMenuItem
            // 
            blockedEmailAddressToolStripMenuItem.Name = "blockedEmailAddressToolStripMenuItem";
            blockedEmailAddressToolStripMenuItem.Size = new Size(257, 26);
            blockedEmailAddressToolStripMenuItem.Text = "Blocked Email Addresses";
            blockedEmailAddressToolStripMenuItem.Click += blockedEmailAddressToolStripMenuItem_Click;
            // 
            // aIAssistantToolStripMenuItem
            // 
            aIAssistantToolStripMenuItem.Name = "aIAssistantToolStripMenuItem";
            aIAssistantToolStripMenuItem.Size = new Size(257, 26);
            aIAssistantToolStripMenuItem.Text = "AI Assistant";
            aIAssistantToolStripMenuItem.Click += aIAssistantToolStripMenuItem_Click;
            // 
            // iceBreakersToolStripMenuItem
            // 
            iceBreakersToolStripMenuItem.Name = "iceBreakersToolStripMenuItem";
            iceBreakersToolStripMenuItem.Size = new Size(257, 26);
            iceBreakersToolStripMenuItem.Text = "Ice Breakers";
            iceBreakersToolStripMenuItem.Click += iceBreakersToolStripMenuItem_Click;
            // 
            // accountsToolStripMenuItem
            // 
            accountsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { addNewAccountToolStripMenuItem });
            accountsToolStripMenuItem.Name = "accountsToolStripMenuItem";
            accountsToolStripMenuItem.Size = new Size(83, 24);
            accountsToolStripMenuItem.Text = "Accounts";
            accountsToolStripMenuItem.Click += accountsToolStripMenuItem_Click;
            // 
            // addNewAccountToolStripMenuItem
            // 
            addNewAccountToolStripMenuItem.Name = "addNewAccountToolStripMenuItem";
            addNewAccountToolStripMenuItem.Size = new Size(212, 26);
            addNewAccountToolStripMenuItem.Text = "Add New Account";
            addNewAccountToolStripMenuItem.Click += addNewAccountToolStripMenuItem_Click;
            // 
            // templatesToolStripMenuItem
            // 
            templatesToolStripMenuItem.Name = "templatesToolStripMenuItem";
            templatesToolStripMenuItem.Size = new Size(91, 24);
            templatesToolStripMenuItem.Text = "Templates";
            templatesToolStripMenuItem.Click += templatesToolStripMenuItem_Click;
            // 
            // proxiesToolStripMenuItem
            // 
            proxiesToolStripMenuItem.Name = "proxiesToolStripMenuItem";
            proxiesToolStripMenuItem.Size = new Size(70, 24);
            proxiesToolStripMenuItem.Text = "Proxies";
            proxiesToolStripMenuItem.Click += proxiesToolStripMenuItem_Click;
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(76, 24);
            settingsToolStripMenuItem.Text = "Settings";
            settingsToolStripMenuItem.Click += settingsToolStripMenuItem_Click;
            // 
            // TubeMailGorilla
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1256, 820);
            Controls.Add(btnSearch);
            Controls.Add(txtLikeCount);
            Controls.Add(lblLikeCount);
            Controls.Add(txtViewCount);
            Controls.Add(lblViewCount);
            Controls.Add(groupBox2);
            Controls.Add(btnBatchSearch);
            Controls.Add(txtNoVideoInChannel);
            Controls.Add(lblNoVideosInChannels);
            Controls.Add(txtPageViewLimit);
            Controls.Add(label1);
            Controls.Add(txtKeyword);
            Controls.Add(lblKeyword);
            Controls.Add(btnClearResults);
            Controls.Add(prgResult);
            Controls.Add(lstResults);
            Controls.Add(label2);
            Controls.Add(cboTemplate);
            Controls.Add(ckTestMode);
            Controls.Add(btnRemoveTest);
            Controls.Add(btnAddTest);
            Controls.Add(cboABTest);
            Controls.Add(ckABTest);
            Controls.Add(ckHtmlMode);
            Controls.Add(lblBody);
            Controls.Add(rtxtBody);
            Controls.Add(txtSubject);
            Controls.Add(lblSubject);
            Controls.Add(btnCommentor);
            Controls.Add(groupBox1);
            Controls.Add(btnFollowUp);
            Controls.Add(grpSummary);
            Controls.Add(btnSendEmail);
            Controls.Add(btnDataControls);
            Controls.Add(menuStrip1);
            Name = "TubeMailGorilla";
            Text = "TubeMailGorilla";
            Load += TubeMailGorilla_Load;
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            grpSummary.ResumeLayout(false);
            grpSummary.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnSearch;
        private TextBox txtLikeCount;
        private Label lblLikeCount;
        private TextBox txtViewCount;
        private Label lblViewCount;
        private GroupBox groupBox2;
        private CheckBox ckHTTPMode;
        private CheckBox ckValidateEmails;
        private CheckBox ckEmailUserName;
        private CheckBox ckGmailAccountOnly;
        private CheckBox ckAccelerateMode;
        private Button btnBatchSearch;
        private TextBox txtNoVideoInChannel;
        private Label lblNoVideosInChannels;
        private TextBox txtPageViewLimit;
        private Label label1;
        private TextBox txtKeyword;
        private Label lblKeyword;
        private Button btnClearResults;
        private ProgressBar prgResult;
        private ListBox lstResults;
        private Label label2;
        private ComboBox cboTemplate;
        private CheckBox ckTestMode;
        private Button btnRemoveTest;
        private Button btnAddTest;
        private ComboBox cboABTest;
        private CheckBox ckABTest;
        private CheckBox ckHtmlMode;
        private Label lblBody;
        private RichTextBox rtxtBody;
        private TextBox txtSubject;
        private Label lblSubject;
        private Button btnCommentor;
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
        private Button btnFollowUp;
        private GroupBox grpSummary;
        private Button btnRefreshSummary;
        private TextBox txtEmailReplies;
        private TextBox txtEmailsForwarded;
        private TextBox txtUniqueEmailsFound;
        private Label lblEmailReplies;
        private Label lblEmailsForward;
        private Label lblUniqueEmailsFound;
        private Button btnSendEmail;
        private Button btnDataControls;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem dataControlsToolStripMenuItem;
        private ToolStripMenuItem dataControlsToolStripMenuItem1;
        private ToolStripMenuItem blockedEmailAddressToolStripMenuItem;
        private ToolStripMenuItem accountsToolStripMenuItem;
        private ToolStripMenuItem templatesToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem proxiesToolStripMenuItem;
        private ToolStripMenuItem addNewAccountToolStripMenuItem;
        private CheckBox ckInsights;
        private ToolStripMenuItem aIAssistantToolStripMenuItem;
        private ToolStripMenuItem iceBreakersToolStripMenuItem;
    }
}