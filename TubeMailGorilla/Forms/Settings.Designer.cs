namespace TubeMailGorilla.Forms
{
    partial class Settings
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
            txtYouTubeDataAPI = new TextBox();
            groupBox4 = new GroupBox();
            label1 = new Label();
            lblIPAddress = new Label();
            txtDomainName = new TextBox();
            groupBox3 = new GroupBox();
            cboPaging = new ComboBox();
            lblPaging = new Label();
            btnSearchBatchFile = new Button();
            grpSearch = new GroupBox();
            groupBox5 = new GroupBox();
            label2 = new Label();
            txtGoogleAi = new TextBox();
            button1 = new Button();
            btnBatchCommentSearch = new Button();
            groupBox4.SuspendLayout();
            groupBox3.SuspendLayout();
            grpSearch.SuspendLayout();
            groupBox5.SuspendLayout();
            SuspendLayout();
            // 
            // txtYouTubeDataAPI
            // 
            txtYouTubeDataAPI.Anchor = AnchorStyles.Right;
            txtYouTubeDataAPI.Location = new Point(137, 30);
            txtYouTubeDataAPI.Name = "txtYouTubeDataAPI";
            txtYouTubeDataAPI.Size = new Size(584, 27);
            txtYouTubeDataAPI.TabIndex = 0;
            // 
            // groupBox4
            // 
            groupBox4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox4.Controls.Add(label1);
            groupBox4.Controls.Add(txtYouTubeDataAPI);
            groupBox4.Location = new Point(12, 157);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(771, 72);
            groupBox4.TabIndex = 9;
            groupBox4.TabStop = false;
            groupBox4.Text = "YouTube Data API";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(21, 33);
            label1.Name = "label1";
            label1.Size = new Size(56, 20);
            label1.TabIndex = 3;
            label1.Text = "ApiKey";
            // 
            // lblIPAddress
            // 
            lblIPAddress.AutoSize = true;
            lblIPAddress.Location = new Point(19, 52);
            lblIPAddress.Name = "lblIPAddress";
            lblIPAddress.Size = new Size(35, 20);
            lblIPAddress.TabIndex = 4;
            lblIPAddress.Text = "URL";
            // 
            // txtDomainName
            // 
            txtDomainName.Anchor = AnchorStyles.Right;
            txtDomainName.Location = new Point(137, 49);
            txtDomainName.Name = "txtDomainName";
            txtDomainName.Size = new Size(584, 27);
            txtDomainName.TabIndex = 2;
            // 
            // groupBox3
            // 
            groupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox3.Controls.Add(lblIPAddress);
            groupBox3.Controls.Add(txtDomainName);
            groupBox3.Location = new Point(16, 319);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(768, 98);
            groupBox3.TabIndex = 10;
            groupBox3.TabStop = false;
            groupBox3.Text = "Web Hosting (TubeMail Gorilla API Hosting)";
            // 
            // cboPaging
            // 
            cboPaging.FormattingEnabled = true;
            cboPaging.Location = new Point(216, 48);
            cboPaging.Name = "cboPaging";
            cboPaging.Size = new Size(510, 28);
            cboPaging.TabIndex = 3;
            cboPaging.SelectedIndexChanged += cboPaging_SelectedIndexChanged;
            // 
            // lblPaging
            // 
            lblPaging.AutoSize = true;
            lblPaging.Location = new Point(26, 51);
            lblPaging.Name = "lblPaging";
            lblPaging.Size = new Size(125, 20);
            lblPaging.TabIndex = 2;
            lblPaging.Text = "Search Limit Type";
            // 
            // btnSearchBatchFile
            // 
            btnSearchBatchFile.Location = new Point(25, 91);
            btnSearchBatchFile.Name = "btnSearchBatchFile";
            btnSearchBatchFile.Size = new Size(247, 29);
            btnSearchBatchFile.TabIndex = 1;
            btnSearchBatchFile.Text = "Generate Batch Search File";
            btnSearchBatchFile.UseVisualStyleBackColor = true;
            btnSearchBatchFile.Click += btnSearchBatchFile_Click;
            // 
            // grpSearch
            // 
            grpSearch.Controls.Add(btnBatchCommentSearch);
            grpSearch.Controls.Add(cboPaging);
            grpSearch.Controls.Add(lblPaging);
            grpSearch.Controls.Add(btnSearchBatchFile);
            grpSearch.Location = new Point(7, 9);
            grpSearch.Name = "grpSearch";
            grpSearch.Size = new Size(776, 139);
            grpSearch.TabIndex = 5;
            grpSearch.TabStop = false;
            grpSearch.Text = "Search Input";
            // 
            // groupBox5
            // 
            groupBox5.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox5.Controls.Add(label2);
            groupBox5.Controls.Add(txtGoogleAi);
            groupBox5.Location = new Point(11, 235);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(771, 72);
            groupBox5.TabIndex = 10;
            groupBox5.TabStop = false;
            groupBox5.Text = "Google AI (Gemini) API Key";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(21, 33);
            label2.Name = "label2";
            label2.Size = new Size(56, 20);
            label2.TabIndex = 3;
            label2.Text = "ApiKey";
            // 
            // txtGoogleAi
            // 
            txtGoogleAi.Anchor = AnchorStyles.Right;
            txtGoogleAi.Location = new Point(138, 26);
            txtGoogleAi.Name = "txtGoogleAi";
            txtGoogleAi.Size = new Size(584, 27);
            txtGoogleAi.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(586, 423);
            button1.Name = "button1";
            button1.Size = new Size(198, 58);
            button1.TabIndex = 11;
            button1.Text = "Save Changes";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // btnBatchCommentSearch
            // 
            btnBatchCommentSearch.Location = new Point(278, 91);
            btnBatchCommentSearch.Name = "btnBatchCommentSearch";
            btnBatchCommentSearch.Size = new Size(348, 29);
            btnBatchCommentSearch.TabIndex = 4;
            btnBatchCommentSearch.Text = "Generate Batch Comment Search File";
            btnBatchCommentSearch.UseVisualStyleBackColor = true;
            btnBatchCommentSearch.Click += btnBatchCommentSearch_Click;
            // 
            // Settings
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(804, 493);
            Controls.Add(button1);
            Controls.Add(groupBox5);
            Controls.Add(groupBox4);
            Controls.Add(groupBox3);
            Controls.Add(grpSearch);
            Name = "Settings";
            Text = "Settings";
            Load += Settings_Load;
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            grpSearch.ResumeLayout(false);
            grpSearch.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TextBox txtYouTubeDataAPI;
        private GroupBox groupBox4;
        private Label label1;
        private Label lblIPAddress;
        private TextBox txtDomainName;
        private GroupBox groupBox3;
        private ComboBox cboPaging;
        private Label lblPaging;
        private Button btnSearchBatchFile;
        private GroupBox grpSearch;
        private GroupBox groupBox5;
        private Label label2;
        private TextBox txtGoogleAi;
        private Button button1;
        private Button btnBatchCommentSearch;
    }
}