namespace TubeMailGorilla.Forms
{
    partial class Blocked_Email_Addresses
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
            btnImportToCSV = new Button();
            btnExportToCSV = new Button();
            btnRemove = new Button();
            btnUpdate = new Button();
            dgvBlocker = new DataGridView();
            btnAdd = new Button();
            txtEmail = new TextBox();
            ((System.ComponentModel.ISupportInitialize)dgvBlocker).BeginInit();
            SuspendLayout();
            // 
            // btnImportToCSV
            // 
            btnImportToCSV.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnImportToCSV.Location = new Point(465, 391);
            btnImportToCSV.Name = "btnImportToCSV";
            btnImportToCSV.Size = new Size(190, 47);
            btnImportToCSV.TabIndex = 13;
            btnImportToCSV.Text = "Import From CSV";
            btnImportToCSV.UseVisualStyleBackColor = true;
            btnImportToCSV.Click += btnImportToCSV_Click;
            // 
            // btnExportToCSV
            // 
            btnExportToCSV.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExportToCSV.Location = new Point(661, 391);
            btnExportToCSV.Name = "btnExportToCSV";
            btnExportToCSV.Size = new Size(127, 47);
            btnExportToCSV.TabIndex = 12;
            btnExportToCSV.Text = "Export To CSV";
            btnExportToCSV.UseVisualStyleBackColor = true;
            btnExportToCSV.Click += btnExportToCSV_Click;
            // 
            // btnRemove
            // 
            btnRemove.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRemove.Location = new Point(145, 391);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(127, 47);
            btnRemove.TabIndex = 11;
            btnRemove.Text = "Remove All";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUpdate.Location = new Point(12, 391);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(127, 47);
            btnUpdate.TabIndex = 10;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // dgvBlocker
            // 
            dgvBlocker.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvBlocker.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvBlocker.Location = new Point(12, 68);
            dgvBlocker.Name = "dgvBlocker";
            dgvBlocker.RowHeadersWidth = 51;
            dgvBlocker.Size = new Size(776, 317);
            dgvBlocker.TabIndex = 9;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Location = new Point(641, 27);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(147, 29);
            btnAdd.TabIndex = 8;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // txtEmail
            // 
            txtEmail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtEmail.Location = new Point(12, 27);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(623, 27);
            txtEmail.TabIndex = 7;
            // 
            // Blocked_Email_Addresses
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 450);
            Controls.Add(btnImportToCSV);
            Controls.Add(btnExportToCSV);
            Controls.Add(btnRemove);
            Controls.Add(btnUpdate);
            Controls.Add(dgvBlocker);
            Controls.Add(btnAdd);
            Controls.Add(txtEmail);
            Name = "Blocked_Email_Addresses";
            Text = "Blocked_Email_Addresses";
            ((System.ComponentModel.ISupportInitialize)dgvBlocker).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnImportToCSV;
        private Button btnExportToCSV;
        private Button btnRemove;
        private Button btnUpdate;
        private DataGridView dgvBlocker;
        private Button btnAdd;
        private TextBox txtEmail;
    }
}