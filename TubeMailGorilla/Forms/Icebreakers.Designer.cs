namespace TubeMailGorilla.Forms
{
    partial class Icebreakers
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
            lblEmail = new Label();
            cboEmail = new ComboBox();
            rtxtMessage = new RichTextBox();
            btnUpdate = new Button();
            lstResult = new ListBox();
            btnClear = new Button();
            SuspendLayout();
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(12, 22);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(46, 20);
            lblEmail.TabIndex = 0;
            lblEmail.Text = "Email";
            // 
            // cboEmail
            // 
            cboEmail.FormattingEnabled = true;
            cboEmail.Location = new Point(78, 19);
            cboEmail.Name = "cboEmail";
            cboEmail.Size = new Size(710, 28);
            cboEmail.TabIndex = 1;
            cboEmail.SelectedIndexChanged += cboEmail_SelectedIndexChanged;
            // 
            // rtxtMessage
            // 
            rtxtMessage.Location = new Point(12, 66);
            rtxtMessage.Name = "rtxtMessage";
            rtxtMessage.Size = new Size(776, 318);
            rtxtMessage.TabIndex = 2;
            rtxtMessage.Text = "";
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(638, 596);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(150, 48);
            btnUpdate.TabIndex = 3;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // lstResult
            // 
            lstResult.FormattingEnabled = true;
            lstResult.Location = new Point(12, 390);
            lstResult.Name = "lstResult";
            lstResult.Size = new Size(776, 184);
            lstResult.TabIndex = 4;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(12, 596);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(150, 48);
            btnClear.TabIndex = 5;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // Icebreakers
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 656);
            Controls.Add(btnClear);
            Controls.Add(lstResult);
            Controls.Add(btnUpdate);
            Controls.Add(rtxtMessage);
            Controls.Add(cboEmail);
            Controls.Add(lblEmail);
            Name = "Icebreakers";
            Text = "Icebreakers";
            Load += Icebreakers_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblEmail;
        private ComboBox cboEmail;
        private RichTextBox rtxtMessage;
        private Button btnUpdate;
        private ListBox lstResult;
        private Button btnClear;
    }
}