namespace TubeMailGorilla.Forms
{
    partial class AI_Assistant
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
            lblFunction = new Label();
            cboFunctions = new ComboBox();
            rtxtPrompt = new RichTextBox();
            btnSubmit = new Button();
            lstResult = new ListBox();
            btnClear = new Button();
            SuspendLayout();
            // 
            // lblFunction
            // 
            lblFunction.AutoSize = true;
            lblFunction.Location = new Point(12, 20);
            lblFunction.Name = "lblFunction";
            lblFunction.Size = new Size(65, 20);
            lblFunction.TabIndex = 0;
            lblFunction.Text = "Function";
            // 
            // cboFunctions
            // 
            cboFunctions.FormattingEnabled = true;
            cboFunctions.Location = new Point(83, 17);
            cboFunctions.Name = "cboFunctions";
            cboFunctions.Size = new Size(705, 28);
            cboFunctions.TabIndex = 1;
            // 
            // rtxtPrompt
            // 
            rtxtPrompt.BackColor = Color.White;
            rtxtPrompt.Location = new Point(12, 62);
            rtxtPrompt.Name = "rtxtPrompt";
            rtxtPrompt.Size = new Size(776, 325);
            rtxtPrompt.TabIndex = 2;
            rtxtPrompt.Text = "";
            // 
            // btnSubmit
            // 
            btnSubmit.Location = new Point(632, 591);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(156, 45);
            btnSubmit.TabIndex = 3;
            btnSubmit.Text = "Submit";
            btnSubmit.UseVisualStyleBackColor = true;
            btnSubmit.Click += btnSubmit_Click;
            // 
            // lstResult
            // 
            lstResult.FormattingEnabled = true;
            lstResult.Location = new Point(12, 393);
            lstResult.Name = "lstResult";
            lstResult.Size = new Size(776, 184);
            lstResult.TabIndex = 4;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(12, 591);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(156, 45);
            btnClear.TabIndex = 5;
            btnClear.Text = "Clear Results";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // AI_Assistant
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 648);
            Controls.Add(btnClear);
            Controls.Add(lstResult);
            Controls.Add(btnSubmit);
            Controls.Add(rtxtPrompt);
            Controls.Add(cboFunctions);
            Controls.Add(lblFunction);
            Name = "AI_Assistant";
            Text = "AI_Assistant";
            Load += AI_Assistant_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblFunction;
        private ComboBox cboFunctions;
        private RichTextBox rtxtPrompt;
        private Button btnSubmit;
        private ListBox lstResult;
        private Button btnClear;
    }
}