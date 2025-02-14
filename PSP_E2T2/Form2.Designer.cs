namespace PSP_E2T2
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            richTextBox1 = new RichTextBox();
            button1 = new Button();
            button2 = new Button();
            richTextBox2 = new RichTextBox();
            button3 = new Button();
            pictureBox1 = new PictureBox();
            helpProvider1 = new HelpProvider();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(12, 63);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.Size = new Size(776, 314);
            richTextBox1.TabIndex = 1;
            richTextBox1.Text = "";
            // 
            // button1
            // 
            button1.BackColor = Color.MediumAquamarine;
            button1.Font = new Font("Segoe UI", 12F);
            button1.ForeColor = SystemColors.WindowText;
            button1.Location = new Point(660, 397);
            button1.Name = "button1";
            button1.Size = new Size(128, 41);
            button1.TabIndex = 2;
            button1.Text = "Testua bidali";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click_1;
            // 
            // button2
            // 
            button2.Font = new Font("Segoe UI", 12F);
            button2.Location = new Point(697, 12);
            button2.Name = "button2";
            button2.Size = new Size(78, 31);
            button2.TabIndex = 3;
            button2.Text = "Bueltatu";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // richTextBox2
            // 
            richTextBox2.Location = new Point(12, 397);
            richTextBox2.Name = "richTextBox2";
            richTextBox2.Size = new Size(642, 41);
            richTextBox2.TabIndex = 4;
            richTextBox2.Text = "";
            richTextBox2.KeyDown += richTextBox2_KeyDown;
            richTextBox2.KeyPress += richTextBox2_KeyPress;
            // 
            // button3
            // 
            button3.Location = new Point(21, 18);
            button3.Name = "button3";
            button3.Size = new Size(89, 23);
            button3.TabIndex = 5;
            button3.Text = "Kontsultatu";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(288, -9);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(196, 66);
            pictureBox1.TabIndex = 6;
            pictureBox1.TabStop = false;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(800, 450);
            Controls.Add(pictureBox1);
            Controls.Add(button3);
            Controls.Add(richTextBox2);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(richTextBox1);
            Margin = new Padding(2);
            Name = "Form2";
            Text = "Mezuak bidali";
            Load += Form2_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private RichTextBox richTextBox1;
        private Button button1;
        private Button button2;
        private RichTextBox richTextBox2;
        private Button button3;
        private PictureBox pictureBox1;
        private HelpProvider helpProvider1;
    }
}