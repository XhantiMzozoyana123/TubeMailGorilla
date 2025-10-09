namespace TubeMailGorilla.Forms
{
    partial class Follow_Up
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
            grpSender = new GroupBox();
            ckTestMode = new CheckBox();
            grpRecipients = new GroupBox();
            rbEmailAddresses = new RadioButton();
            rbKeywords = new RadioButton();
            cboRecipients = new ComboBox();
            btnRemoveTest = new Button();
            btnAddTest = new Button();
            cboABTest = new ComboBox();
            ckABTest = new CheckBox();
            ckHtmlMode = new CheckBox();
            lblBody = new Label();
            rtxtBody = new RichTextBox();
            txtSubject = new TextBox();
            lblSubject = new Label();
            groupBox1 = new GroupBox();
            cboEmailAddress = new ComboBox();
            grpInbox = new GroupBox();
            dgvInboxer = new DataGridView();
            checkBox1 = new CheckBox();
            label2 = new Label();
            cboTemplate = new ComboBox();
            comboBox1 = new ComboBox();
            checkBox2 = new CheckBox();
            checkBox3 = new CheckBox();
            btnRefreshInbox = new Button();
            btnSendEmail = new Button();
            grpSender.SuspendLayout();
            grpRecipients.SuspendLayout();
            groupBox1.SuspendLayout();
            grpInbox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvInboxer).BeginInit();
            SuspendLayout();
            // 
            // grpSender
            // 
            grpSender.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpSender.Controls.Add(checkBox1);
            grpSender.Controls.Add(label2);
            grpSender.Controls.Add(cboTemplate);
            grpSender.Controls.Add(comboBox1);
            grpSender.Controls.Add(checkBox2);
            grpSender.Controls.Add(checkBox3);
            grpSender.Controls.Add(ckTestMode);
            grpSender.Controls.Add(grpRecipients);
            grpSender.Controls.Add(btnRemoveTest);
            grpSender.Controls.Add(btnAddTest);
            grpSender.Controls.Add(cboABTest);
            grpSender.Controls.Add(ckABTest);
            grpSender.Controls.Add(ckHtmlMode);
            grpSender.Controls.Add(lblBody);
            grpSender.Controls.Add(rtxtBody);
            grpSender.Controls.Add(txtSubject);
            grpSender.Controls.Add(lblSubject);
            grpSender.Location = new Point(637, 12);
            grpSender.Name = "grpSender";
            grpSender.Size = new Size(572, 766);
            grpSender.TabIndex = 5;
            grpSender.TabStop = false;
            grpSender.Text = "Sender";
            // 
            // ckTestMode
            // 
            ckTestMode.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ckTestMode.AutoSize = true;
            ckTestMode.Location = new Point(22, 1416);
            ckTestMode.Name = "ckTestMode";
            ckTestMode.Size = new Size(100, 24);
            ckTestMode.TabIndex = 14;
            ckTestMode.Text = "Test Mode";
            ckTestMode.UseVisualStyleBackColor = true;
            // 
            // grpRecipients
            // 
            grpRecipients.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpRecipients.Controls.Add(rbEmailAddresses);
            grpRecipients.Controls.Add(rbKeywords);
            grpRecipients.Controls.Add(cboRecipients);
            grpRecipients.Location = new Point(20, 92);
            grpRecipients.Name = "grpRecipients";
            grpRecipients.Size = new Size(521, 100);
            grpRecipients.TabIndex = 9;
            grpRecipients.TabStop = false;
            grpRecipients.Text = "Recipients";
            // 
            // rbEmailAddresses
            // 
            rbEmailAddresses.AutoSize = true;
            rbEmailAddresses.Location = new Point(180, 26);
            rbEmailAddresses.Name = "rbEmailAddresses";
            rbEmailAddresses.Size = new Size(200, 24);
            rbEmailAddresses.TabIndex = 5;
            rbEmailAddresses.TabStop = true;
            rbEmailAddresses.Text = "Select by email addresses";
            rbEmailAddresses.UseVisualStyleBackColor = true;
            // 
            // rbKeywords
            // 
            rbKeywords.AutoSize = true;
            rbKeywords.Location = new Point(18, 26);
            rbKeywords.Name = "rbKeywords";
            rbKeywords.Size = new Size(156, 24);
            rbKeywords.TabIndex = 4;
            rbKeywords.TabStop = true;
            rbKeywords.Text = "Select by keywords";
            rbKeywords.UseVisualStyleBackColor = true;
            // 
            // cboRecipients
            // 
            cboRecipients.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboRecipients.FormattingEnabled = true;
            cboRecipients.Location = new Point(18, 56);
            cboRecipients.Name = "cboRecipients";
            cboRecipients.Size = new Size(497, 28);
            cboRecipients.TabIndex = 2;
            // 
            // btnRemoveTest
            // 
            btnRemoveTest.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnRemoveTest.Location = new Point(886, 1441);
            btnRemoveTest.Name = "btnRemoveTest";
            btnRemoveTest.Size = new Size(101, 29);
            btnRemoveTest.TabIndex = 8;
            btnRemoveTest.Text = "Remove";
            btnRemoveTest.UseVisualStyleBackColor = true;
            // 
            // btnAddTest
            // 
            btnAddTest.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAddTest.Location = new Point(779, 1441);
            btnAddTest.Name = "btnAddTest";
            btnAddTest.Size = new Size(101, 29);
            btnAddTest.TabIndex = 7;
            btnAddTest.Text = "Add";
            btnAddTest.UseVisualStyleBackColor = true;
            // 
            // cboABTest
            // 
            cboABTest.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            cboABTest.FormattingEnabled = true;
            cboABTest.Location = new Point(150, 1442);
            cboABTest.Name = "cboABTest";
            cboABTest.Size = new Size(623, 28);
            cboABTest.TabIndex = 6;
            // 
            // ckABTest
            // 
            ckABTest.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ckABTest.AutoSize = true;
            ckABTest.Location = new Point(20, 1446);
            ckABTest.Name = "ckABTest";
            ckABTest.Size = new Size(90, 24);
            ckABTest.TabIndex = 5;
            ckABTest.Text = "A/B Test ";
            ckABTest.UseVisualStyleBackColor = true;
            // 
            // ckHtmlMode
            // 
            ckHtmlMode.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ckHtmlMode.AutoSize = true;
            ckHtmlMode.Location = new Point(22, 1392);
            ckHtmlMode.Name = "ckHtmlMode";
            ckHtmlMode.Size = new Size(113, 24);
            ckHtmlMode.TabIndex = 4;
            ckHtmlMode.Text = "HTML Mode";
            ckHtmlMode.UseVisualStyleBackColor = true;
            // 
            // lblBody
            // 
            lblBody.AutoSize = true;
            lblBody.Location = new Point(20, 208);
            lblBody.Name = "lblBody";
            lblBody.Size = new Size(43, 20);
            lblBody.TabIndex = 3;
            lblBody.Text = "Body";
            // 
            // rtxtBody
            // 
            rtxtBody.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtxtBody.Location = new Point(20, 243);
            rtxtBody.Name = "rtxtBody";
            rtxtBody.Size = new Size(521, 394);
            rtxtBody.TabIndex = 2;
            rtxtBody.Text = "";
            // 
            // txtSubject
            // 
            txtSubject.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSubject.Location = new Point(18, 59);
            txtSubject.Name = "txtSubject";
            txtSubject.Size = new Size(523, 27);
            txtSubject.TabIndex = 1;
            // 
            // lblSubject
            // 
            lblSubject.AutoSize = true;
            lblSubject.Location = new Point(18, 36);
            lblSubject.Name = "lblSubject";
            lblSubject.Size = new Size(58, 20);
            lblSubject.TabIndex = 0;
            lblSubject.Text = "Subject";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(cboEmailAddress);
            groupBox1.Location = new Point(12, 24);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(613, 74);
            groupBox1.TabIndex = 10;
            groupBox1.TabStop = false;
            groupBox1.Text = "Active Forwarding Email Address";
            // 
            // cboEmailAddress
            // 
            cboEmailAddress.FormattingEnabled = true;
            cboEmailAddress.Location = new Point(21, 32);
            cboEmailAddress.Name = "cboEmailAddress";
            cboEmailAddress.Size = new Size(574, 28);
            cboEmailAddress.TabIndex = 0;
            // 
            // grpInbox
            // 
            grpInbox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            grpInbox.Controls.Add(dgvInboxer);
            grpInbox.Location = new Point(12, 104);
            grpInbox.Name = "grpInbox";
            grpInbox.Size = new Size(619, 674);
            grpInbox.TabIndex = 9;
            grpInbox.TabStop = false;
            grpInbox.Text = "Inbox";
            // 
            // dgvInboxer
            // 
            dgvInboxer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dgvInboxer.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvInboxer.Location = new Point(12, 26);
            dgvInboxer.Name = "dgvInboxer";
            dgvInboxer.RowHeadersWidth = 51;
            dgvInboxer.Size = new Size(583, 625);
            dgvInboxer.TabIndex = 0;
            // 
            // checkBox1
            // 
            checkBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(20, 673);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(100, 24);
            checkBox1.TabIndex = 20;
            checkBox1.Text = "Test Mode";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(20, 735);
            label2.Name = "label2";
            label2.Size = new Size(115, 20);
            label2.TabIndex = 19;
            label2.Text = "Select Template";
            // 
            // cboTemplate
            // 
            cboTemplate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboTemplate.FormattingEnabled = true;
            cboTemplate.Location = new Point(184, 727);
            cboTemplate.Name = "cboTemplate";
            cboTemplate.Size = new Size(351, 28);
            cboTemplate.TabIndex = 18;
            // 
            // comboBox1
            // 
            comboBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(184, 693);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(351, 28);
            comboBox1.TabIndex = 17;
            // 
            // checkBox2
            // 
            checkBox2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            checkBox2.AutoSize = true;
            checkBox2.Location = new Point(20, 697);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(90, 24);
            checkBox2.TabIndex = 16;
            checkBox2.Text = "A/B Test ";
            checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            checkBox3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            checkBox3.AutoSize = true;
            checkBox3.Location = new Point(20, 643);
            checkBox3.Name = "checkBox3";
            checkBox3.Size = new Size(113, 24);
            checkBox3.TabIndex = 15;
            checkBox3.Text = "HTML Mode";
            checkBox3.UseVisualStyleBackColor = true;
            // 
            // btnRefreshInbox
            // 
            btnRefreshInbox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRefreshInbox.Location = new Point(12, 779);
            btnRefreshInbox.Name = "btnRefreshInbox";
            btnRefreshInbox.Size = new Size(151, 56);
            btnRefreshInbox.TabIndex = 12;
            btnRefreshInbox.Text = "Refresh Inbox";
            btnRefreshInbox.UseVisualStyleBackColor = true;
            // 
            // btnSendEmail
            // 
            btnSendEmail.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSendEmail.Location = new Point(1046, 784);
            btnSendEmail.Name = "btnSendEmail";
            btnSendEmail.Size = new Size(163, 56);
            btnSendEmail.TabIndex = 11;
            btnSendEmail.Text = "Send Email";
            btnSendEmail.UseVisualStyleBackColor = true;
            // 
            // Follow_Up
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1221, 847);
            Controls.Add(btnRefreshInbox);
            Controls.Add(btnSendEmail);
            Controls.Add(groupBox1);
            Controls.Add(grpInbox);
            Controls.Add(grpSender);
            Name = "Follow_Up";
            Text = "Follow_Up";
            grpSender.ResumeLayout(false);
            grpSender.PerformLayout();
            grpRecipients.ResumeLayout(false);
            grpRecipients.PerformLayout();
            groupBox1.ResumeLayout(false);
            grpInbox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvInboxer).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpSender;
        private CheckBox ckTestMode;
        private GroupBox grpRecipients;
        private RadioButton rbEmailAddresses;
        private RadioButton rbKeywords;
        private ComboBox cboRecipients;
        private Button btnRemoveTest;
        private Button btnAddTest;
        private ComboBox cboABTest;
        private CheckBox ckABTest;
        private CheckBox ckHtmlMode;
        private Label lblBody;
        private RichTextBox rtxtBody;
        private TextBox txtSubject;
        private Label lblSubject;
        private GroupBox groupBox1;
        private ComboBox cboEmailAddress;
        private GroupBox grpInbox;
        private DataGridView dgvInboxer;
        private CheckBox checkBox1;
        private Label label2;
        private ComboBox cboTemplate;
        private ComboBox comboBox1;
        private CheckBox checkBox2;
        private CheckBox checkBox3;
        private Button btnRefreshInbox;
        private Button btnSendEmail;
    }
}