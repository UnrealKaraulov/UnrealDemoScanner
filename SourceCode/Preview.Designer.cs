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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // CurrentTimeTextBox
            // 
            this.CurrentTimeTextBox.BackColor = System.Drawing.Color.AliceBlue;
            this.CurrentTimeTextBox.Location = new System.Drawing.Point(315, 418);
            this.CurrentTimeTextBox.Name = "CurrentTimeTextBox";
            this.CurrentTimeTextBox.Size = new System.Drawing.Size(100, 20);
            this.CurrentTimeTextBox.TabIndex = 0;
            // 
            // NextTimeBtn
            // 
            this.NextTimeBtn.Location = new System.Drawing.Point(444, 417);
            this.NextTimeBtn.Name = "NextTimeBtn";
            this.NextTimeBtn.Size = new System.Drawing.Size(47, 20);
            this.NextTimeBtn.TabIndex = 1;
            this.NextTimeBtn.Text = ">>";
            this.NextTimeBtn.UseVisualStyleBackColor = true;
            // 
            // PrevTimeBtn
            // 
            this.PrevTimeBtn.Location = new System.Drawing.Point(239, 417);
            this.PrevTimeBtn.Name = "PrevTimeBtn";
            this.PrevTimeBtn.Size = new System.Drawing.Size(47, 20);
            this.PrevTimeBtn.TabIndex = 1;
            this.PrevTimeBtn.Text = "<<";
            this.PrevTimeBtn.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(-1, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(802, 368);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // Preview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.PrevTimeBtn);
            this.Controls.Add(this.NextTimeBtn);
            this.Controls.Add(this.CurrentTimeTextBox);
            this.Name = "Preview";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Preview Frames";
            this.Shown += new System.EventHandler(this.Preview_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox CurrentTimeTextBox;
        private System.Windows.Forms.Button NextTimeBtn;
        private System.Windows.Forms.Button PrevTimeBtn;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}