namespace DemoScanner
{
    partial class Preview
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
            this.CurrentTimeTextBox = new System.Windows.Forms.TextBox();
            this.NextTimeBtn = new System.Windows.Forms.Button();
            this.PrevTimeBtn = new System.Windows.Forms.Button();
            this.drawGraphBox = new System.Windows.Forms.PictureBox();
            this.drawKeyboardBox = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.showUCMD = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.drawGraphBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.drawKeyboardBox)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // CurrentTimeTextBox
            // 
            this.CurrentTimeTextBox.BackColor = System.Drawing.Color.AliceBlue;
            this.CurrentTimeTextBox.Location = new System.Drawing.Point(612, 415);
            this.CurrentTimeTextBox.Name = "CurrentTimeTextBox";
            this.CurrentTimeTextBox.Size = new System.Drawing.Size(100, 20);
            this.CurrentTimeTextBox.TabIndex = 0;
            this.CurrentTimeTextBox.TextChanged += new System.EventHandler(this.CurrentTimeTextBox_TextChanged);
            this.CurrentTimeTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CurrentTimeTextBox_KeyUp);
            // 
            // NextTimeBtn
            // 
            this.NextTimeBtn.Location = new System.Drawing.Point(741, 414);
            this.NextTimeBtn.Name = "NextTimeBtn";
            this.NextTimeBtn.Size = new System.Drawing.Size(47, 20);
            this.NextTimeBtn.TabIndex = 1;
            this.NextTimeBtn.Text = ">>";
            this.NextTimeBtn.UseVisualStyleBackColor = true;
            this.NextTimeBtn.Click += new System.EventHandler(this.NextTimeBtn_Click);
            // 
            // PrevTimeBtn
            // 
            this.PrevTimeBtn.Location = new System.Drawing.Point(536, 414);
            this.PrevTimeBtn.Name = "PrevTimeBtn";
            this.PrevTimeBtn.Size = new System.Drawing.Size(47, 20);
            this.PrevTimeBtn.TabIndex = 1;
            this.PrevTimeBtn.Text = "<<";
            this.PrevTimeBtn.UseVisualStyleBackColor = true;
            this.PrevTimeBtn.Click += new System.EventHandler(this.PrevTimeBtn_Click);
            // 
            // drawGraphBox
            // 
            this.drawGraphBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.drawGraphBox.Location = new System.Drawing.Point(39, 12);
            this.drawGraphBox.Name = "drawGraphBox";
            this.drawGraphBox.Size = new System.Drawing.Size(749, 182);
            this.drawGraphBox.TabIndex = 2;
            this.drawGraphBox.TabStop = false;
            this.drawGraphBox.Paint += new System.Windows.Forms.PaintEventHandler(this.drawGraphBox_Paint);
            // 
            // drawKeyboardBox
            // 
            this.drawKeyboardBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.drawKeyboardBox.Location = new System.Drawing.Point(39, 200);
            this.drawKeyboardBox.Name = "drawKeyboardBox";
            this.drawKeyboardBox.Size = new System.Drawing.Size(749, 182);
            this.drawKeyboardBox.TabIndex = 2;
            this.drawKeyboardBox.TabStop = false;
            this.drawKeyboardBox.Paint += new System.Windows.Forms.PaintEventHandler(this.drawGraphBox_Paint);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.checkBox3);
            this.panel1.Controls.Add(this.checkBox2);
            this.panel1.Controls.Add(this.showUCMD);
            this.panel1.Location = new System.Drawing.Point(40, 402);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(360, 45);
            this.panel1.TabIndex = 3;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(237, 15);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(80, 17);
            this.checkBox3.TabIndex = 4;
            this.checkBox3.Text = "checkBox1";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(126, 15);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(80, 17);
            this.checkBox2.TabIndex = 4;
            this.checkBox2.Text = "checkBox1";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // showUCMD
            // 
            this.showUCMD.AutoSize = true;
            this.showUCMD.Location = new System.Drawing.Point(18, 15);
            this.showUCMD.Name = "showUCMD";
            this.showUCMD.Size = new System.Drawing.Size(80, 17);
            this.showUCMD.TabIndex = 4;
            this.showUCMD.Text = "checkBox1";
            this.showUCMD.UseVisualStyleBackColor = true;
            // 
            // Preview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(838, 450);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.drawKeyboardBox);
            this.Controls.Add(this.drawGraphBox);
            this.Controls.Add(this.PrevTimeBtn);
            this.Controls.Add(this.NextTimeBtn);
            this.Controls.Add(this.CurrentTimeTextBox);
            this.Name = "Preview";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Preview Frames";
            this.Load += new System.EventHandler(this.Preview_Load);
            ((System.ComponentModel.ISupportInitialize)(this.drawGraphBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.drawKeyboardBox)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox CurrentTimeTextBox;
        private System.Windows.Forms.Button NextTimeBtn;
        private System.Windows.Forms.Button PrevTimeBtn;
        private System.Windows.Forms.PictureBox drawGraphBox;
        private System.Windows.Forms.PictureBox drawKeyboardBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox showUCMD;
    }
}