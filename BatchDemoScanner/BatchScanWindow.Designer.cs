namespace BatchDemoScanner
{
    partial class BatchScanWindow
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "",
            "",
            "",
            ""}, -1);
            this.DemoScanList = new System.Windows.Forms.ListView();
            this.demoPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.demoCheated = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.SelectedThreadCount = new System.Windows.Forms.TextBox();
            this.BreakScanBtn = new System.Windows.Forms.Button();
            this.StartScanBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // DemoScanList
            // 
            this.DemoScanList.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.DemoScanList.AllowDrop = true;
            this.DemoScanList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DemoScanList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.demoPath,
            this.demoCheated});
            this.DemoScanList.GridLines = true;
            this.DemoScanList.HideSelection = false;
            this.DemoScanList.HotTracking = true;
            this.DemoScanList.HoverSelection = true;
            listViewItem1.StateImageIndex = 0;
            this.DemoScanList.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.DemoScanList.Location = new System.Drawing.Point(28, 26);
            this.DemoScanList.Name = "DemoScanList";
            this.DemoScanList.ShowGroups = false;
            this.DemoScanList.ShowItemToolTips = true;
            this.DemoScanList.Size = new System.Drawing.Size(562, 333);
            this.DemoScanList.TabIndex = 1;
            this.DemoScanList.TabStop = false;
            this.DemoScanList.UseCompatibleStateImageBehavior = false;
            this.DemoScanList.View = System.Windows.Forms.View.Details;
            this.DemoScanList.DragDrop += new System.Windows.Forms.DragEventHandler(this.DemoScanList_DragDrop);
            this.DemoScanList.DragEnter += new System.Windows.Forms.DragEventHandler(this.DemoScanList_DragEnter);
            // 
            // demoPath
            // 
            this.demoPath.Text = "Demo path";
            this.demoPath.Width = 447;
            // 
            // demoCheated
            // 
            this.demoCheated.Text = "Status";
            this.demoCheated.Width = 102;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "";
            this.columnHeader1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader1.Width = 1;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 387);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Threads:";
            // 
            // SelectedThreadCount
            // 
            this.SelectedThreadCount.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.SelectedThreadCount.Location = new System.Drawing.Point(116, 384);
            this.SelectedThreadCount.Name = "SelectedThreadCount";
            this.SelectedThreadCount.Size = new System.Drawing.Size(40, 20);
            this.SelectedThreadCount.TabIndex = 2;
            this.SelectedThreadCount.TabStop = false;
            this.SelectedThreadCount.Text = "10";
            // 
            // BreakScanBtn
            // 
            this.BreakScanBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.BreakScanBtn.Location = new System.Drawing.Point(454, 382);
            this.BreakScanBtn.Name = "BreakScanBtn";
            this.BreakScanBtn.Size = new System.Drawing.Size(75, 23);
            this.BreakScanBtn.TabIndex = 4;
            this.BreakScanBtn.TabStop = false;
            this.BreakScanBtn.Text = "Stop";
            this.BreakScanBtn.UseVisualStyleBackColor = true;
            this.BreakScanBtn.Click += new System.EventHandler(this.BreakScanBtn_Click);
            // 
            // StartScanBtn
            // 
            this.StartScanBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.StartScanBtn.Location = new System.Drawing.Point(352, 382);
            this.StartScanBtn.Name = "StartScanBtn";
            this.StartScanBtn.Size = new System.Drawing.Size(75, 23);
            this.StartScanBtn.TabIndex = 3;
            this.StartScanBtn.TabStop = false;
            this.StartScanBtn.Text = "Start";
            this.StartScanBtn.UseVisualStyleBackColor = true;
            this.StartScanBtn.Click += new System.EventHandler(this.StartScanBtn_Click);
            // 
            // BatchScanWindow
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 421);
            this.Controls.Add(this.StartScanBtn);
            this.Controls.Add(this.BreakScanBtn);
            this.Controls.Add(this.SelectedThreadCount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DemoScanList);
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "BatchScanWindow";
            this.Text = "Batch Demo Scanner  1.0";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BatchScanWindow_FormClosed);
            this.Load += new System.EventHandler(this.BatchScanWindow_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.BatchScanWindow_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.BatchScanWindow_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox SelectedThreadCount;
        private System.Windows.Forms.Button BreakScanBtn;
        private System.Windows.Forms.Button StartScanBtn;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        public System.Windows.Forms.ColumnHeader demoPath;
        public System.Windows.Forms.ColumnHeader demoCheated;
        public System.Windows.Forms.ListView DemoScanList;
    }
}

