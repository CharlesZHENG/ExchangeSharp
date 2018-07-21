namespace BitKiwiForm
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.BtnSave = new System.Windows.Forms.Button();
            this.BtnDelete = new System.Windows.Forms.Button();
            this.BtnGo = new System.Windows.Forms.Button();
            this.BoxRangeTime = new System.Windows.Forms.ComboBox();
            this.BoxRangePrice = new System.Windows.Forms.ComboBox();
            this.BoxHold = new System.Windows.Forms.ComboBox();
            this.BoxLossPoint = new System.Windows.Forms.ComboBox();
            this.BoxBaseCoin = new System.Windows.Forms.ComboBox();
            this.BtnAdd = new System.Windows.Forms.Button();
            this.Box = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.TxtTimeBuy = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.TxtTargetCoin = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.dataGrid = new System.Windows.Forms.DataGridView();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.BtnSave);
            this.splitContainer1.Panel1.Controls.Add(this.BtnDelete);
            this.splitContainer1.Panel1.Controls.Add(this.BtnGo);
            this.splitContainer1.Panel1.Controls.Add(this.BoxRangeTime);
            this.splitContainer1.Panel1.Controls.Add(this.BoxRangePrice);
            this.splitContainer1.Panel1.Controls.Add(this.BoxHold);
            this.splitContainer1.Panel1.Controls.Add(this.BoxLossPoint);
            this.splitContainer1.Panel1.Controls.Add(this.BoxBaseCoin);
            this.splitContainer1.Panel1.Controls.Add(this.BtnAdd);
            this.splitContainer1.Panel1.Controls.Add(this.Box);
            this.splitContainer1.Panel1.Controls.Add(this.label8);
            this.splitContainer1.Panel1.Controls.Add(this.TxtTimeBuy);
            this.splitContainer1.Panel1.Controls.Add(this.label7);
            this.splitContainer1.Panel1.Controls.Add(this.label6);
            this.splitContainer1.Panel1.Controls.Add(this.label5);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.TxtTargetCoin);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1316, 729);
            this.splitContainer1.SplitterDistance = 300;
            this.splitContainer1.TabIndex = 30;
            // 
            // BtnSave
            // 
            this.BtnSave.Location = new System.Drawing.Point(143, 373);
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Size = new System.Drawing.Size(75, 23);
            this.BtnSave.TabIndex = 46;
            this.BtnSave.Text = "保存";
            this.BtnSave.UseVisualStyleBackColor = true;
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // BtnDelete
            // 
            this.BtnDelete.Location = new System.Drawing.Point(35, 373);
            this.BtnDelete.Name = "BtnDelete";
            this.BtnDelete.Size = new System.Drawing.Size(75, 23);
            this.BtnDelete.TabIndex = 45;
            this.BtnDelete.Text = "删除";
            this.BtnDelete.UseVisualStyleBackColor = true;
            this.BtnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // BtnGo
            // 
            this.BtnGo.Location = new System.Drawing.Point(143, 332);
            this.BtnGo.Name = "BtnGo";
            this.BtnGo.Size = new System.Drawing.Size(75, 23);
            this.BtnGo.TabIndex = 44;
            this.BtnGo.Text = "开始";
            this.BtnGo.UseVisualStyleBackColor = true;
            this.BtnGo.Click += new System.EventHandler(this.BtnGo_Click);
            // 
            // BoxRangeTime
            // 
            this.BoxRangeTime.FormattingEnabled = true;
            this.BoxRangeTime.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5"});
            this.BoxRangeTime.Location = new System.Drawing.Point(119, 185);
            this.BoxRangeTime.Name = "BoxRangeTime";
            this.BoxRangeTime.Size = new System.Drawing.Size(100, 20);
            this.BoxRangeTime.TabIndex = 43;
            // 
            // BoxRangePrice
            // 
            this.BoxRangePrice.FormattingEnabled = true;
            this.BoxRangePrice.Items.AddRange(new object[] {
            "5",
            "10",
            "15",
            "20"});
            this.BoxRangePrice.Location = new System.Drawing.Point(119, 136);
            this.BoxRangePrice.Name = "BoxRangePrice";
            this.BoxRangePrice.Size = new System.Drawing.Size(100, 20);
            this.BoxRangePrice.TabIndex = 42;
            // 
            // BoxHold
            // 
            this.BoxHold.FormattingEnabled = true;
            this.BoxHold.Items.AddRange(new object[] {
            "10",
            "20",
            "30",
            "40",
            "50",
            "60",
            "70",
            "80",
            "90",
            "100"});
            this.BoxHold.Location = new System.Drawing.Point(119, 261);
            this.BoxHold.Name = "BoxHold";
            this.BoxHold.Size = new System.Drawing.Size(100, 20);
            this.BoxHold.TabIndex = 41;
            // 
            // BoxLossPoint
            // 
            this.BoxLossPoint.FormattingEnabled = true;
            this.BoxLossPoint.Items.AddRange(new object[] {
            "5",
            "8"});
            this.BoxLossPoint.Location = new System.Drawing.Point(119, 216);
            this.BoxLossPoint.Name = "BoxLossPoint";
            this.BoxLossPoint.Size = new System.Drawing.Size(100, 20);
            this.BoxLossPoint.TabIndex = 40;
            // 
            // BoxBaseCoin
            // 
            this.BoxBaseCoin.FormattingEnabled = true;
            this.BoxBaseCoin.Items.AddRange(new object[] {
            "ETH",
            "USDT",
            "BTC",
            "HT"});
            this.BoxBaseCoin.Location = new System.Drawing.Point(119, 99);
            this.BoxBaseCoin.Name = "BoxBaseCoin";
            this.BoxBaseCoin.Size = new System.Drawing.Size(100, 20);
            this.BoxBaseCoin.TabIndex = 39;
            // 
            // BtnAdd
            // 
            this.BtnAdd.Location = new System.Drawing.Point(35, 333);
            this.BtnAdd.Name = "BtnAdd";
            this.BtnAdd.Size = new System.Drawing.Size(75, 23);
            this.BtnAdd.TabIndex = 38;
            this.BtnAdd.Text = "添加";
            this.BtnAdd.UseVisualStyleBackColor = true;
            this.BtnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // Box
            // 
            this.Box.FormattingEnabled = true;
            this.Box.Items.AddRange(new object[] {
            "火币",
            "币安",
            "FCoin"});
            this.Box.Location = new System.Drawing.Point(119, 24);
            this.Box.Name = "Box";
            this.Box.Size = new System.Drawing.Size(100, 20);
            this.Box.TabIndex = 37;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(33, 305);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 12);
            this.label8.TabIndex = 36;
            this.label8.Text = "买入点";
            // 
            // TxtTimeBuy
            // 
            this.TxtTimeBuy.Location = new System.Drawing.Point(119, 296);
            this.TxtTimeBuy.Name = "TxtTimeBuy";
            this.TxtTimeBuy.Size = new System.Drawing.Size(100, 21);
            this.TxtTimeBuy.TabIndex = 35;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(33, 264);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 12);
            this.label7.TabIndex = 34;
            this.label7.Text = "推荐持仓比例";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(33, 224);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 33;
            this.label6.Text = "止损比例";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(33, 188);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 32;
            this.label5.Text = "预判时间";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(33, 144);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 31;
            this.label4.Text = "预判涨幅";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 30;
            this.label3.Text = "基币种";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(33, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 29;
            this.label2.Text = "目标币种";
            // 
            // TxtTargetCoin
            // 
            this.TxtTargetCoin.Location = new System.Drawing.Point(119, 59);
            this.TxtTargetCoin.Name = "TxtTargetCoin";
            this.TxtTargetCoin.Size = new System.Drawing.Size(100, 21);
            this.TxtTargetCoin.TabIndex = 28;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 27;
            this.label1.Text = "平台";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.dataGrid);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer2.Size = new System.Drawing.Size(1012, 729);
            this.splitContainer2.SplitterDistance = 397;
            this.splitContainer2.TabIndex = 31;
            // 
            // dataGrid
            // 
            this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGrid.Location = new System.Drawing.Point(0, 0);
            this.dataGrid.MultiSelect = false;
            this.dataGrid.Name = "dataGrid";
            this.dataGrid.ReadOnly = true;
            this.dataGrid.RowTemplate.Height = 23;
            this.dataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGrid.Size = new System.Drawing.Size(1012, 397);
            this.dataGrid.TabIndex = 31;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(1012, 328);
            this.webBrowser1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1316, 729);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button BtnGo;
        private System.Windows.Forms.ComboBox BoxRangeTime;
        private System.Windows.Forms.ComboBox BoxRangePrice;
        private System.Windows.Forms.ComboBox BoxHold;
        private System.Windows.Forms.ComboBox BoxLossPoint;
        private System.Windows.Forms.ComboBox BoxBaseCoin;
        private System.Windows.Forms.Button BtnAdd;
        private System.Windows.Forms.ComboBox Box;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox TxtTimeBuy;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TxtTargetCoin;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BtnSave;
        private System.Windows.Forms.Button BtnDelete;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dataGrid;
        private System.Windows.Forms.WebBrowser webBrowser1;
    }
}

