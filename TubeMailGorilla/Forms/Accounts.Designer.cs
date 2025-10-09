namespace TubeMailGorilla.Forms
{
    partial class Accounts
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
            dgvCredentials = new DataGridView();
            btUpdate = new Button();
            btnDeleteRecords = new Button();
            btnAddRecord = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvCredentials).BeginInit();
            SuspendLayout();
            // 
            // dgvCredentials
            // 
            dgvCredentials.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvCredentials.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCredentials.Location = new Point(12, 12);
            dgvCredentials.Name = "dgvCredentials";
            dgvCredentials.RowHeadersWidth = 51;
            dgvCredentials.Size = new Size(980, 488);
            dgvCredentials.TabIndex = 7;
            // 
            // btUpdate
            // 
            btUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btUpdate.Location = new Point(303, 506);
            btUpdate.Name = "btUpdate";
            btUpdate.Size = new Size(161, 53);
            btUpdate.TabIndex = 6;
            btUpdate.Text = "Update Accounts";
            btUpdate.UseVisualStyleBackColor = true;
            btUpdate.Click += btUpdate_Click;
            // 
            // btnDeleteRecords
            // 
            btnDeleteRecords.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDeleteRecords.Location = new Point(135, 506);
            btnDeleteRecords.Name = "btnDeleteRecords";
            btnDeleteRecords.Size = new Size(162, 53);
            btnDeleteRecords.TabIndex = 5;
            btnDeleteRecords.Text = "Delete Records";
            btnDeleteRecords.UseVisualStyleBackColor = true;
            btnDeleteRecords.Click += btnDeleteRecords_Click;
            // 
            // btnAddRecord
            // 
            btnAddRecord.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAddRecord.Location = new Point(12, 506);
            btnAddRecord.Name = "btnAddRecord";
            btnAddRecord.Size = new Size(117, 53);
            btnAddRecord.TabIndex = 4;
            btnAddRecord.Text = "Add Record";
            btnAddRecord.UseVisualStyleBackColor = true;
            btnAddRecord.Click += btnAddRecord_Click;
            // 
            // Accounts
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1004, 571);
            Controls.Add(dgvCredentials);
            Controls.Add(btUpdate);
            Controls.Add(btnDeleteRecords);
            Controls.Add(btnAddRecord);
            Name = "Accounts";
            Text = "Accounts";
            ((System.ComponentModel.ISupportInitialize)dgvCredentials).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvCredentials;
        private Button btUpdate;
        private Button btnDeleteRecords;
        private Button btnAddRecord;
    }
}