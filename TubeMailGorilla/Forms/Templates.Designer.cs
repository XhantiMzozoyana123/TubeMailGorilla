namespace TubeMailGorilla.Forms
{
    partial class Templates
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
            txtName = new TextBox();
            lblName = new Label();
            label2 = new Label();
            cboTemplate = new ComboBox();
            ckHtmlMode = new CheckBox();
            lblBody = new Label();
            rtxtBody = new RichTextBox();
            txtSubject = new TextBox();
            lblSubject = new Label();
            btnUpdate = new Button();
            btnAdd = new Button();
            btnDeleteAll = new Button();
            btnDelete = new Button();
            SuspendLayout();
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtName.Location = new Point(14, 37);
            txtName.Name = "txtName";
            txtName.Size = new Size(880, 27);
            txtName.TabIndex = 37;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(14, 14);
            lblName.Name = "lblName";
            lblName.Size = new Size(49, 20);
            lblName.TabIndex = 36;
            lblName.Text = "Name";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(161, 595);
            label2.Name = "label2";
            label2.Size = new Size(115, 20);
            label2.TabIndex = 35;
            label2.Text = "Select Template";
            // 
            // cboTemplate
            // 
            cboTemplate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cboTemplate.FormattingEnabled = true;
            cboTemplate.Location = new Point(282, 592);
            cboTemplate.Name = "cboTemplate";
            cboTemplate.Size = new Size(612, 28);
            cboTemplate.TabIndex = 34;
            cboTemplate.SelectedIndexChanged += cboTemplate_SelectedIndexChanged;
            // 
            // ckHtmlMode
            // 
            ckHtmlMode.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ckHtmlMode.AutoSize = true;
            ckHtmlMode.Location = new Point(15, 592);
            ckHtmlMode.Name = "ckHtmlMode";
            ckHtmlMode.Size = new Size(113, 24);
            ckHtmlMode.TabIndex = 33;
            ckHtmlMode.Text = "HTML Mode";
            ckHtmlMode.UseVisualStyleBackColor = true;
            // 
            // lblBody
            // 
            lblBody.AutoSize = true;
            lblBody.Location = new Point(14, 135);
            lblBody.Name = "lblBody";
            lblBody.Size = new Size(43, 20);
            lblBody.TabIndex = 32;
            lblBody.Text = "Body";
            // 
            // rtxtBody
            // 
            rtxtBody.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtxtBody.Location = new Point(14, 170);
            rtxtBody.Name = "rtxtBody";
            rtxtBody.Size = new Size(880, 404);
            rtxtBody.TabIndex = 31;
            rtxtBody.Text = "";
            // 
            // txtSubject
            // 
            txtSubject.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSubject.Location = new Point(14, 99);
            txtSubject.Name = "txtSubject";
            txtSubject.Size = new Size(880, 27);
            txtSubject.TabIndex = 30;
            // 
            // lblSubject
            // 
            lblSubject.AutoSize = true;
            lblSubject.Location = new Point(14, 76);
            lblSubject.Name = "lblSubject";
            lblSubject.Size = new Size(58, 20);
            lblSubject.TabIndex = 29;
            lblSubject.Text = "Subject";
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnUpdate.Location = new Point(646, 626);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(121, 40);
            btnUpdate.TabIndex = 28;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAdd.Location = new Point(773, 626);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(121, 40);
            btnAdd.TabIndex = 27;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnDeleteAll
            // 
            btnDeleteAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteAll.Location = new Point(137, 626);
            btnDeleteAll.Name = "btnDeleteAll";
            btnDeleteAll.Size = new Size(121, 40);
            btnDeleteAll.TabIndex = 26;
            btnDeleteAll.Text = "Delete All";
            btnDeleteAll.UseVisualStyleBackColor = true;
            btnDeleteAll.Click += btnDeleteAll_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDelete.Location = new Point(10, 626);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(121, 40);
            btnDelete.TabIndex = 25;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // Templates
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(906, 678);
            Controls.Add(txtName);
            Controls.Add(lblName);
            Controls.Add(label2);
            Controls.Add(cboTemplate);
            Controls.Add(ckHtmlMode);
            Controls.Add(lblBody);
            Controls.Add(rtxtBody);
            Controls.Add(txtSubject);
            Controls.Add(lblSubject);
            Controls.Add(btnUpdate);
            Controls.Add(btnAdd);
            Controls.Add(btnDeleteAll);
            Controls.Add(btnDelete);
            Name = "Templates";
            Text = "Templates";
            Load += Templates_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtName;
        private Label lblName;
        private Label label2;
        private ComboBox cboTemplate;
        private CheckBox ckHtmlMode;
        private Label lblBody;
        private RichTextBox rtxtBody;
        private TextBox txtSubject;
        private Label lblSubject;
        private Button btnUpdate;
        private Button btnAdd;
        private Button btnDeleteAll;
        private Button btnDelete;
    }
}