namespace TubeMailGorilla.Forms
{
    partial class Proxies
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
            txtUrl = new TextBox();
            label1 = new Label();
            btnAdd = new Button();
            dgvProxies = new DataGridView();
            btnUpdate = new Button();
            btnDelete = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvProxies).BeginInit();
            SuspendLayout();
            // 
            // txtUrl
            // 
            txtUrl.Location = new Point(83, 12);
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new Size(533, 27);
            txtUrl.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(22, 15);
            label1.Name = "label1";
            label1.Size = new Size(35, 20);
            label1.TabIndex = 1;
            label1.Text = "URL";
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(622, 12);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(166, 29);
            btnAdd.TabIndex = 2;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // dgvProxies
            // 
            dgvProxies.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProxies.Location = new Point(22, 45);
            dgvProxies.Name = "dgvProxies";
            dgvProxies.RowHeadersWidth = 51;
            dgvProxies.Size = new Size(766, 350);
            dgvProxies.TabIndex = 3;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(450, 401);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(166, 37);
            btnUpdate.TabIndex = 4;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(622, 401);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(166, 37);
            btnDelete.TabIndex = 5;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // Proxies
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 450);
            Controls.Add(btnDelete);
            Controls.Add(btnUpdate);
            Controls.Add(dgvProxies);
            Controls.Add(btnAdd);
            Controls.Add(label1);
            Controls.Add(txtUrl);
            Name = "Proxies";
            Text = "Proxies";
            Load += Proxies_Load;
            ((System.ComponentModel.ISupportInitialize)dgvProxies).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtUrl;
        private Label label1;
        private Button btnAdd;
        private DataGridView dgvProxies;
        private Button btnUpdate;
        private Button btnDelete;
    }
}