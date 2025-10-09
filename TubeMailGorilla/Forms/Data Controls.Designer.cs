namespace TubeMailGorilla.Forms
{
    partial class Data_Controls
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
            components = new System.ComponentModel.Container();
            btnImportFromCSV = new Button();
            btnBlockedEmailAddresses = new Button();
            btnDeleteRecords = new Button();
            btnUpdateRecords = new Button();
            emailersBindingSource = new BindingSource(components);
            btnExportToCSV = new Button();
            dgvEmailer = new DataGridView();
            lblSource = new Label();
            cboSource = new ComboBox();
            btnAiAssistant = new Button();
            ((System.ComponentModel.ISupportInitialize)emailersBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvEmailer).BeginInit();
            SuspendLayout();
            // 
            // btnImportFromCSV
            // 
            btnImportFromCSV.Location = new Point(978, 468);
            btnImportFromCSV.Name = "btnImportFromCSV";
            btnImportFromCSV.Size = new Size(163, 51);
            btnImportFromCSV.TabIndex = 11;
            btnImportFromCSV.Text = "Import From CSV";
            btnImportFromCSV.UseVisualStyleBackColor = true;
            btnImportFromCSV.Click += btnImportFromCSV_Click;
            // 
            // btnBlockedEmailAddresses
            // 
            btnBlockedEmailAddresses.Location = new Point(357, 468);
            btnBlockedEmailAddresses.Name = "btnBlockedEmailAddresses";
            btnBlockedEmailAddresses.Size = new Size(233, 51);
            btnBlockedEmailAddresses.TabIndex = 9;
            btnBlockedEmailAddresses.Text = "Blocked Email Addresses";
            btnBlockedEmailAddresses.UseVisualStyleBackColor = true;
            btnBlockedEmailAddresses.Click += btnBlockedEmailAddresses_Click;
            // 
            // btnDeleteRecords
            // 
            btnDeleteRecords.Location = new Point(183, 469);
            btnDeleteRecords.Name = "btnDeleteRecords";
            btnDeleteRecords.Size = new Size(168, 51);
            btnDeleteRecords.TabIndex = 8;
            btnDeleteRecords.Text = "Delete Records";
            btnDeleteRecords.UseVisualStyleBackColor = true;
            btnDeleteRecords.Click += btnDeleteRecords_Click;
            // 
            // btnUpdateRecords
            // 
            btnUpdateRecords.Location = new Point(9, 469);
            btnUpdateRecords.Name = "btnUpdateRecords";
            btnUpdateRecords.Size = new Size(168, 51);
            btnUpdateRecords.TabIndex = 7;
            btnUpdateRecords.Text = "Update Records";
            btnUpdateRecords.UseVisualStyleBackColor = true;
            btnUpdateRecords.Click += btnUpdateRecords_Click;
            // 
            // btnExportToCSV
            // 
            btnExportToCSV.Location = new Point(1147, 469);
            btnExportToCSV.Name = "btnExportToCSV";
            btnExportToCSV.Size = new Size(163, 51);
            btnExportToCSV.TabIndex = 10;
            btnExportToCSV.Text = "Export To CSV";
            btnExportToCSV.UseVisualStyleBackColor = true;
            btnExportToCSV.Click += btnExportToCSV_Click;
            // 
            // dgvEmailer
            // 
            dgvEmailer.AllowDrop = true;
            dgvEmailer.AllowUserToOrderColumns = true;
            dgvEmailer.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvEmailer.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvEmailer.ImeMode = ImeMode.On;
            dgvEmailer.Location = new Point(9, 52);
            dgvEmailer.Name = "dgvEmailer";
            dgvEmailer.RowHeadersWidth = 51;
            dgvEmailer.Size = new Size(1301, 411);
            dgvEmailer.TabIndex = 6;
            dgvEmailer.VirtualMode = true;
            // 
            // lblSource
            // 
            lblSource.AutoSize = true;
            lblSource.Location = new Point(12, 20);
            lblSource.Name = "lblSource";
            lblSource.Size = new Size(57, 20);
            lblSource.TabIndex = 12;
            lblSource.Text = "Source:";
            // 
            // cboSource
            // 
            cboSource.FormattingEnabled = true;
            cboSource.Location = new Point(75, 17);
            cboSource.Name = "cboSource";
            cboSource.Size = new Size(1232, 28);
            cboSource.TabIndex = 13;
            cboSource.SelectedIndexChanged += cboSource_SelectedIndexChanged;
            // 
            // btnAiAssistant
            // 
            btnAiAssistant.Location = new Point(596, 468);
            btnAiAssistant.Name = "btnAiAssistant";
            btnAiAssistant.Size = new Size(233, 51);
            btnAiAssistant.TabIndex = 14;
            btnAiAssistant.Text = "AI Assistant";
            btnAiAssistant.UseVisualStyleBackColor = true;
            btnAiAssistant.Click += btnAiAssistant_Click;
            // 
            // Data_Controls
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1319, 528);
            Controls.Add(btnAiAssistant);
            Controls.Add(cboSource);
            Controls.Add(lblSource);
            Controls.Add(btnImportFromCSV);
            Controls.Add(btnBlockedEmailAddresses);
            Controls.Add(btnDeleteRecords);
            Controls.Add(btnUpdateRecords);
            Controls.Add(btnExportToCSV);
            Controls.Add(dgvEmailer);
            Name = "Data_Controls";
            Text = "Data_Controls";
            Load += Data_Controls_Load;
            ((System.ComponentModel.ISupportInitialize)emailersBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvEmailer).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnImportFromCSV;
        private Button btnBlockedEmailAddresses;
        private Button btnDeleteRecords;
        private Button btnUpdateRecords;
        private BindingSource emailersBindingSource;
        private Button btnExportToCSV;
        private DataGridView dgvEmailer;
        private Label lblSource;
        private ComboBox cboSource;
        private Button btnAiAssistant;
    }
}