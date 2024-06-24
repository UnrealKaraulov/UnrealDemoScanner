namespace DemoScanner
{
    partial class EntitiesPreviewWindow
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
            this.components = new System.ComponentModel.Container();
            this.entPreviewPicture = new System.Windows.Forms.PictureBox();
            this.EntSelector = new System.Windows.Forms.ComboBox();
            this.frameSelector = new System.Windows.Forms.TrackBar();
            this.EntData = new System.Windows.Forms.ListBox();
            this.playBtn = new System.Windows.Forms.Button();
            this.fpsSpeed = new System.Windows.Forms.TextBox();
            this.fpsLabel = new System.Windows.Forms.Label();
            this.NextBtn = new System.Windows.Forms.Button();
            this.PrevBtn = new System.Windows.Forms.Button();
            this.fpsTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.entPreviewPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.frameSelector)).BeginInit();
            this.SuspendLayout();
            // 
            // entPreviewPicture
            // 
            this.entPreviewPicture.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.entPreviewPicture.Location = new System.Drawing.Point(3, 1);
            this.entPreviewPicture.Name = "entPreviewPicture";
            this.entPreviewPicture.Size = new System.Drawing.Size(464, 383);
            this.entPreviewPicture.TabIndex = 0;
            this.entPreviewPicture.TabStop = false;
            this.entPreviewPicture.Paint += new System.Windows.Forms.PaintEventHandler(this.entPreviewPicture_Paint);
            // 
            // EntSelector
            // 
            this.EntSelector.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EntSelector.FormattingEnabled = true;
            this.EntSelector.Location = new System.Drawing.Point(483, 12);
            this.EntSelector.Name = "EntSelector";
            this.EntSelector.Size = new System.Drawing.Size(209, 21);
            this.EntSelector.TabIndex = 1;
            // 
            // frameSelector
            // 
            this.frameSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.frameSelector.Location = new System.Drawing.Point(3, 390);
            this.frameSelector.Name = "frameSelector";
            this.frameSelector.Size = new System.Drawing.Size(689, 45);
            this.frameSelector.TabIndex = 2;
            // 
            // EntData
            // 
            this.EntData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EntData.FormattingEnabled = true;
            this.EntData.Location = new System.Drawing.Point(483, 37);
            this.EntData.Name = "EntData";
            this.EntData.Size = new System.Drawing.Size(209, 290);
            this.EntData.TabIndex = 3;
            // 
            // playBtn
            // 
            this.playBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.playBtn.Location = new System.Drawing.Point(601, 337);
            this.playBtn.Name = "playBtn";
            this.playBtn.Size = new System.Drawing.Size(75, 23);
            this.playBtn.TabIndex = 4;
            this.playBtn.Text = "Play/Pause";
            this.playBtn.UseVisualStyleBackColor = true;
            this.playBtn.Click += new System.EventHandler(this.playBtn_Click);
            // 
            // fpsSpeed
            // 
            this.fpsSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fpsSpeed.Location = new System.Drawing.Point(522, 339);
            this.fpsSpeed.Name = "fpsSpeed";
            this.fpsSpeed.Size = new System.Drawing.Size(73, 20);
            this.fpsSpeed.TabIndex = 5;
            this.fpsSpeed.Text = "100";
            this.fpsSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.fpsSpeed.TextChanged += new System.EventHandler(this.fpsSpeed_TextChanged);
            // 
            // fpsLabel
            // 
            this.fpsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fpsLabel.AutoSize = true;
            this.fpsLabel.Location = new System.Drawing.Point(489, 342);
            this.fpsLabel.Name = "fpsLabel";
            this.fpsLabel.Size = new System.Drawing.Size(27, 13);
            this.fpsLabel.TabIndex = 6;
            this.fpsLabel.Text = "FPS";
            // 
            // NextBtn
            // 
            this.NextBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NextBtn.Location = new System.Drawing.Point(601, 365);
            this.NextBtn.Name = "NextBtn";
            this.NextBtn.Size = new System.Drawing.Size(75, 23);
            this.NextBtn.TabIndex = 4;
            this.NextBtn.Text = "Next";
            this.NextBtn.UseVisualStyleBackColor = true;
            this.NextBtn.Click += new System.EventHandler(this.playBtn_Click);
            // 
            // PrevBtn
            // 
            this.PrevBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PrevBtn.Location = new System.Drawing.Point(492, 365);
            this.PrevBtn.Name = "PrevBtn";
            this.PrevBtn.Size = new System.Drawing.Size(75, 23);
            this.PrevBtn.TabIndex = 4;
            this.PrevBtn.Text = "Prev";
            this.PrevBtn.UseVisualStyleBackColor = true;
            this.PrevBtn.Click += new System.EventHandler(this.playBtn_Click);
            // 
            // fpsTimer
            // 
            this.fpsTimer.Enabled = true;
            this.fpsTimer.Interval = 10;
            this.fpsTimer.Tick += new System.EventHandler(this.fpsTimer_Tick);
            // 
            // EntitiesPreviewWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 435);
            this.Controls.Add(this.fpsLabel);
            this.Controls.Add(this.fpsSpeed);
            this.Controls.Add(this.PrevBtn);
            this.Controls.Add(this.NextBtn);
            this.Controls.Add(this.playBtn);
            this.Controls.Add(this.EntData);
            this.Controls.Add(this.frameSelector);
            this.Controls.Add(this.EntSelector);
            this.Controls.Add(this.entPreviewPicture);
            this.MinimumSize = new System.Drawing.Size(720, 474);
            this.Name = "EntitiesPreviewWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "EntitiesPreviewWindow";
            this.Load += new System.EventHandler(this.EntitiesPreviewWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.entPreviewPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.frameSelector)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox entPreviewPicture;
        private System.Windows.Forms.ComboBox EntSelector;
        private System.Windows.Forms.TrackBar frameSelector;
        private System.Windows.Forms.ListBox EntData;
        private System.Windows.Forms.Button playBtn;
        private System.Windows.Forms.TextBox fpsSpeed;
        private System.Windows.Forms.Label fpsLabel;
        private System.Windows.Forms.Button NextBtn;
        private System.Windows.Forms.Button PrevBtn;
        private System.Windows.Forms.Timer fpsTimer;
    }
}