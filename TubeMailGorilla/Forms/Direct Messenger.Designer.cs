namespace TubeMailGorilla.Forms
{
    partial class Direct_Messenger
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
            listBox1 = new ListBox();
            button1 = new Button();
            label1 = new Label();
            button2 = new Button();
            richTextBox1 = new RichTextBox();
            label2 = new Label();
            button3 = new Button();
            button4 = new Button();
            SuspendLayout();
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(12, 35);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(929, 184);
            listBox1.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(12, 597);
            button1.Name = "button1";
            button1.Size = new Size(150, 41);
            button1.TabIndex = 1;
            button1.Text = "Send Email Directly";
            button1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(117, 20);
            label1.TabIndex = 2;
            label1.Text = "Email Addresses";
            // 
            // button2
            // 
            button2.Location = new Point(791, 597);
            button2.Name = "button2";
            button2.Size = new Size(150, 41);
            button2.TabIndex = 3;
            button2.Text = "Mark as Contacted";
            button2.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(12, 253);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(929, 338);
            richTextBox1.TabIndex = 4;
            richTextBox1.Text = "";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 230);
            label2.Name = "label2";
            label2.Size = new Size(133, 20);
            label2.TabIndex = 5;
            label2.Text = "Message Template";
            label2.Click += label2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(364, 597);
            button3.Name = "button3";
            button3.Size = new Size(150, 41);
            button3.TabIndex = 6;
            button3.Text = "Open Links";
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Location = new Point(168, 597);
            button4.Name = "button4";
            button4.Size = new Size(190, 41);
            button4.TabIndex = 7;
            button4.Text = "AI Generate Template";
            button4.UseVisualStyleBackColor = true;
            // 
            // Direct_Messenger
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(953, 650);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(label2);
            Controls.Add(richTextBox1);
            Controls.Add(button2);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(listBox1);
            Name = "Direct_Messenger";
            Text = "Direct_Messenger";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox listBox1;
        private Button button1;
        private Label label1;
        private Button button2;
        private RichTextBox richTextBox1;
        private Label label2;
        private Button button3;
        private Button button4;
    }
}