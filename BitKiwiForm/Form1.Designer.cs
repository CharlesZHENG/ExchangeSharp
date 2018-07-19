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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.TxtTargetCoin = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.TxtTimeBuy = new System.Windows.Forms.TextBox();
            this.Box = new System.Windows.Forms.ComboBox();
            this.BtnAdd = new System.Windows.Forms.Button();
            this.BoxBaseCoin = new System.Windows.Forms.ComboBox();
            this.BoxLossPoint = new System.Windows.Forms.ComboBox();
            this.BoxHold = new System.Windows.Forms.ComboBox();
            this.BoxRangePrice = new System.Windows.Forms.ComboBox();
            this.BoxRangeTime = new System.Windows.Forms.ComboBox();
            this.BtnGo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(57, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "平台";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(57, 114);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "目标币种";
            // 
            // TxtTargetCoin
            // 
            this.TxtTargetCoin.Location = new System.Drawing.Point(143, 105);
            this.TxtTargetCoin.Name = "TxtTargetCoin";
            this.TxtTargetCoin.Size = new System.Drawing.Size(100, 21);
            this.TxtTargetCoin.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(57, 153);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "基币种";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(57, 190);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "预判涨幅";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(57, 234);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "预判时间";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(57, 270);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "止损比例";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(57, 310);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 12);
            this.label7.TabIndex = 13;
            this.label7.Text = "推荐持仓比例";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(57, 351);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 12);
            this.label8.TabIndex = 15;
            this.label8.Text = "买入点";
            // 
            // TxtTimeBuy
            // 
            this.TxtTimeBuy.Location = new System.Drawing.Point(143, 342);
            this.TxtTimeBuy.Name = "TxtTimeBuy";
            this.TxtTimeBuy.Size = new System.Drawing.Size(100, 21);
            this.TxtTimeBuy.TabIndex = 14;
            // 
            // Box
            // 
            this.Box.FormattingEnabled = true;
            this.Box.Items.AddRange(new object[] {
            "火币",
            "币安",
            "FCoin"});
            this.Box.Location = new System.Drawing.Point(143, 70);
            this.Box.Name = "Box";
            this.Box.Size = new System.Drawing.Size(100, 20);
            this.Box.TabIndex = 17;
            // 
            // BtnAdd
            // 
            this.BtnAdd.Location = new System.Drawing.Point(59, 379);
            this.BtnAdd.Name = "BtnAdd";
            this.BtnAdd.Size = new System.Drawing.Size(75, 23);
            this.BtnAdd.TabIndex = 18;
            this.BtnAdd.Text = "添加";
            this.BtnAdd.UseVisualStyleBackColor = true;
            this.BtnAdd.Click += new System.EventHandler(this.BtnSet_Click);
            // 
            // BoxBaseCoin
            // 
            this.BoxBaseCoin.FormattingEnabled = true;
            this.BoxBaseCoin.Items.AddRange(new object[] {
            "ETH",
            "USDT",
            "BTC",
            "HT"});
            this.BoxBaseCoin.Location = new System.Drawing.Point(143, 145);
            this.BoxBaseCoin.Name = "BoxBaseCoin";
            this.BoxBaseCoin.Size = new System.Drawing.Size(100, 20);
            this.BoxBaseCoin.TabIndex = 21;
            // 
            // BoxLossPoint
            // 
            this.BoxLossPoint.FormattingEnabled = true;
            this.BoxLossPoint.Items.AddRange(new object[] {
            "5",
            "8"});
            this.BoxLossPoint.Location = new System.Drawing.Point(143, 262);
            this.BoxLossPoint.Name = "BoxLossPoint";
            this.BoxLossPoint.Size = new System.Drawing.Size(100, 20);
            this.BoxLossPoint.TabIndex = 22;
            // 
            // BoxHold
            // 
            this.BoxHold.FormattingEnabled = true;
            this.BoxHold.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.BoxHold.Location = new System.Drawing.Point(143, 307);
            this.BoxHold.Name = "BoxHold";
            this.BoxHold.Size = new System.Drawing.Size(100, 20);
            this.BoxHold.TabIndex = 23;
            // 
            // BoxRangePrice
            // 
            this.BoxRangePrice.FormattingEnabled = true;
            this.BoxRangePrice.Items.AddRange(new object[] {
            "5",
            "10",
            "15",
            "20"});
            this.BoxRangePrice.Location = new System.Drawing.Point(143, 182);
            this.BoxRangePrice.Name = "BoxRangePrice";
            this.BoxRangePrice.Size = new System.Drawing.Size(100, 20);
            this.BoxRangePrice.TabIndex = 24;
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
            this.BoxRangeTime.Location = new System.Drawing.Point(143, 231);
            this.BoxRangeTime.Name = "BoxRangeTime";
            this.BoxRangeTime.Size = new System.Drawing.Size(100, 20);
            this.BoxRangeTime.TabIndex = 25;
            // 
            // BtnGo
            // 
            this.BtnGo.Location = new System.Drawing.Point(167, 378);
            this.BtnGo.Name = "BtnGo";
            this.BtnGo.Size = new System.Drawing.Size(75, 23);
            this.BtnGo.TabIndex = 26;
            this.BtnGo.Text = "开始";
            this.BtnGo.UseVisualStyleBackColor = true;
            this.BtnGo.Click += new System.EventHandler(this.BtnGo_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(313, 434);
            this.Controls.Add(this.BtnGo);
            this.Controls.Add(this.BoxRangeTime);
            this.Controls.Add(this.BoxRangePrice);
            this.Controls.Add(this.BoxHold);
            this.Controls.Add(this.BoxLossPoint);
            this.Controls.Add(this.BoxBaseCoin);
            this.Controls.Add(this.BtnAdd);
            this.Controls.Add(this.Box);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.TxtTimeBuy);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TxtTargetCoin);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TxtTargetCoin;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox TxtTimeBuy;
        private System.Windows.Forms.ComboBox Box;
        private System.Windows.Forms.Button BtnAdd;
        private System.Windows.Forms.ComboBox BoxBaseCoin;
        private System.Windows.Forms.ComboBox BoxLossPoint;
        private System.Windows.Forms.ComboBox BoxHold;
        private System.Windows.Forms.ComboBox BoxRangePrice;
        private System.Windows.Forms.ComboBox BoxRangeTime;
        private System.Windows.Forms.Button BtnGo;
    }
}

