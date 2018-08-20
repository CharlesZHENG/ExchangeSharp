using ExchangeSharp;
using ExchangeSharp.API.Exchanges;
using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BitKiwiForm.Strategy;

namespace BitKiwiForm
{
    public enum InputStatus
    {
        None = 0,
        Doing = 1,
        Done = 2
    }

    public enum InputSrc
    {
        UI = 0,
        StrategyStatistics = 1
    }

    public partial class Form1 : Form
    {
        private string[] eth;
        private IExchangeAPI exchange = new ExchangeHuobiAPI();
        private decimal init_usdt;
        private List<Input> inputList = new List<Input>();
        private int sleeptime = 10000;//休眠时间，默认为30s
        //new ExchangeHuobiAPI();
        private string[] usdt;
        public Form1()
        {
            InitializeComponent();
        }

        public delegate void DelegateSetAmount(string strAmount, string strGain);

        public delegate void DelegateSetCotnent(List<Input> list);

        public delegate void DelegateSetCotnent1(List<TableDisplay> list);

        public void SetAmount(string strAmount, string strGain)
        {
            if (this.dataGrid.InvokeRequired)
            {
                Invoke(new DelegateSetAmount(SetAmount), strAmount, strGain);
            }
            else
            {
                this.textBox1.Text = strAmount;
                this.textBox2.Text = strGain;
            }
        }

        public void SetPrompInfo(List<Input> list)
        {
            if (this.dataGrid.InvokeRequired)
            {
                Invoke(new DelegateSetCotnent(SetPrompInfo), list);
            }
            else
            {
                this.dataGrid.DataSource = null;
                this.dataGrid.DataSource = list;
                this.dataGrid.Refresh();
            }
        }

        public void SetPrompInfoT(List<TableDisplay> list)
        {
            if (this.dataGrid.InvokeRequired)
            {
                Invoke(new DelegateSetCotnent1(SetPrompInfoT), list);
            }
            else
            {
                this.dataGrid.DataSource = null;
                this.dataGrid.DataSource = list;
                this.dataGrid.Columns[0].HeaderText = "持有币种";
                this.dataGrid.Columns[1].HeaderText = "止损率";
                this.dataGrid.Columns[2].HeaderText = "止盈率";
                this.dataGrid.Columns[3].HeaderText = "盈利率";
                this.dataGrid.Columns[4].HeaderText = "持仓比例";
                this.dataGrid.Columns[5].HeaderText = "运行情况";
                this.dataGrid.Refresh();
                AutoSizeColumn(this.dataGrid);
            }
        }

        public List<TableDisplay> TableAdapter(List<Input> inputList)
        {
            List<TableDisplay> tableList = new List<TableDisplay>();
            foreach (var input in inputList)
            {
                TableDisplay row = new TableDisplay();
                // 持仓币种的名称
                row.Tokenname = input.TargetCurrency + "/" + input.BaseCurrency;
                // 每个币种的持仓比例
                row.Hold = input.Hold + "%";
                // 止损点
                row.LossPoint = input.LossPoint;
                // 止盈点
                row.SellPoint = input.GainPointMin.ToString();
                // 盈利率
                row.Gain = input.Gain.ToString();
                if (input.Status.ToString().Equals("None"))
                {
                    row.Status = "尚未启动";
                }
                else if (input.Status.ToString().Equals("Doing"))
                {
                    row.Status = "正在运行";
                }
                else if (input.Status.ToString().Equals("Done"))
                {
                    row.Status = "订单已完成";
                }
                tableList.Add(row);
            }
            return tableList;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var input = new Input();
            input.BaseCurrency = BoxBaseCoin.SelectedItem.ToString().ToLower();
            input.Hold = BoxHold.SelectedItem.ToString();
            input.LossPoint = BoxLossPoint.Text;
            input.GainPointMin = BoxRangePrice.Text;
            input.GainPointMax = "120";
            input.RangeTimeMin = BoxRangeTime.SelectedItem.ToString();
            //input.RangeTimeMax = TxtTimeMax.Text;
            input.TargetCurrency = TxtTargetCoin.Text.ToLower();
            input.WebSite = Box.SelectedItem.ToString();
            input.Status = InputStatus.None;
            input.InitalTime = DateTime.Now;
            input.Precision = txtPrecision.Text.Trim();
            input.InputSrc = InputSrc.UI;
            var validator = new InputValidator();
            ValidationResult results = validator.Validate(input);
            if (results.IsValid)
            {
                input.ExchangeApi = InitExchange(input);
                inputList.Add(input);
                List<TableDisplay> displayList = this.TableAdapter(inputList);
                SetPrompInfoT(displayList);
                var data = JsonConvert.SerializeObject(inputList);
                File.WriteAllText("data.json", data);
            }
            else
            {
                var error = results.Errors.Select(o => o.ErrorMessage).ToList();
                MessageBox.Show($"输入参数有误{JsonConvert.SerializeObject(error)}");
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGrid.SelectedRows.Count > 0)
            {
                var index = dataGrid.CurrentRow.Index;
                inputList.RemoveAt(index);
                dataGrid.DataSource = null;
                dataGrid.DataSource = this.TableAdapter(inputList);
                this.dataGrid.Columns[0].HeaderText = "持有币种";
                this.dataGrid.Columns[1].HeaderText = "止损率";
                this.dataGrid.Columns[2].HeaderText = "止盈率";
                this.dataGrid.Columns[3].HeaderText = "盈利率";
                this.dataGrid.Columns[4].HeaderText = "持仓比例";
                this.dataGrid.Columns[5].HeaderText = "运行情况";
                AutoSizeColumn(this.dataGrid);
            }
        }

        private void AddToList(int stime, string marketid, string webSite)
        {
            List<double> dataList = new List<double>();
            var s = new Statistics();
            Task.Run(() =>
            {
                while (true)
                {
                    if (dataList.Count < 500)
                    {
                        var data = s.GetGata(marketid, exchange);
                        dataList.Add((double)data.Price);
                        Thread.Sleep(stime);
                    }
                    else
                    {
                        var result = s.GetStatisticsResult(dataList.ToArray());
                        if (result.IsNormal)
                        {
                            //添加到list
                            var input = new Input();
                            input.BaseCurrency = marketid.Split(new[] { '/' })[0];
                            input.Hold = "20";
                            input.LossPoint = (100 * 3 * result.StdDev / result.Mean).ToString();
                            input.GainPointMin = (100 * 2 * result.StdDev / result.Mean).ToString();
                            input.GainPointMax = "120";
                            input.RangeTimeMin = "500";//s
                            input.InitalPrice = (decimal)(result.Mean - 2 * result.StdDev);
                            input.TargetCurrency = marketid.Split(new[] { '/' })[1];
                            input.WebSite = webSite;
                            input.Status = InputStatus.None;
                            input.InitalTime = DateTime.Now;
                            input.Precision = "2";
                            input.InputSrc = InputSrc.StrategyStatistics;
                            var validator = new InputValidator();
                            ValidationResult results = validator.Validate(input);
                            if (results.IsValid)
                            {
                                input.ExchangeApi = InitExchange(input);
                                inputList.Add(input);
                                List<TableDisplay> displayList = this.TableAdapter(inputList);
                                SetPrompInfoT(displayList);
                                var data = JsonConvert.SerializeObject(inputList);
                                File.WriteAllText("data.json", data);
                            }
                            else
                            {
                                var error = results.Errors.Select(o => o.ErrorMessage).ToList();
                                MessageBox.Show($@"输入参数有误{JsonConvert.SerializeObject(error)}");
                            }
                        }
                        dataList.Clear();
                    }
                }
            });
        }

        private void BtnGo_Click(object sender, EventArgs e)
        {
            AddToList(500, "eth/usdt", "火币");
            Task.Run(() =>
            {
                while (true)
                {
                    var preList = inputList.Where(a => a.Status == InputStatus.None).ToList();
                    var doingList = inputList.Where(a => a.Status == InputStatus.Doing).ToList();
                    if (preList.Any())
                    {
                        foreach (var input in preList)
                        {
                            input.ExchangeApi = InitExchange(input);
                            DoBuy(true, input).GetAwaiter().GetResult();
                        }
                    }
                    else if (doingList.Any())
                    {
                        var tasks = new Task[doingList.Count];
                        Parallel.For(0, doingList.Count, i =>
                        {
                            inputList[i].ExchangeApi = InitExchange(doingList[i]);
                            tasks[i] = DoBuy(false, doingList[i]);
                        });
                        Task.WhenAll(tasks).GetAwaiter().GetResult();
                    }
                    inputList.ForEach(a =>
                    {
                        if (a.InitalPrice != 0)
                        {
                            a.Gain = (a.SellPrice - a.InitalPrice) / (a.InitalPrice) * 100;
                        }
                    });

                    SetPrompInfoT(this.TableAdapter(inputList));

                    //SetPrompInfo(inputList);

                    if (inputList.All(a => a.Status == InputStatus.Done))
                    {
                        break;
                    }
                    Thread.Sleep(sleeptime);
                }
            });
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var data = JsonConvert.SerializeObject(inputList);
            File.WriteAllText("data.json", data);
            dataGrid.DataSource = this.TableAdapter(inputList);
            this.dataGrid.Columns[0].HeaderText = "持有币种";
            this.dataGrid.Columns[1].HeaderText = "止损率";
            this.dataGrid.Columns[2].HeaderText = "止盈率";
            this.dataGrid.Columns[3].HeaderText = "盈利率";
            this.dataGrid.Columns[4].HeaderText = "持仓比例";
            AutoSizeColumn(this.dataGrid);
        }

        private Task<decimal> CalculateUSDT(KeyValuePair<string, decimal> account, decimal ethUsdt)
        {
            var amount = 0m;
            // 如果持有币种为usdt
            if ("usdt".Equals(account.Key))
            {
                amount = account.Value;
            }
            else
            {
                // 如果该币在usdt交易区，则折算为usdt
                if (this.usdt.Contains(account.Key.ToUpper()))
                {
                    var depth = exchange.GetOrderBook(account.Key + "/usdt", 1);
                    var bids = depth.Bids.OrderByDescending(a => a.Value.Price);
                    amount = account.Value * bids.ElementAt(0).Value.Price;
                }
                // 如果该币在eth交易区，则先折算为eth，再折算为usdt
                else if (this.eth.Contains(account.Key.ToUpper()))
                {
                    var depth = exchange.GetOrderBook(account.Key + "/eth", 1);
                    var bids = depth.Bids.OrderByDescending(a => a.Value.Price);
                    amount += account.Value * bids.ElementAt(0).Value.Price * ethUsdt;
                }
            }
            return Task.FromResult<decimal>(amount);
        }

        private Task DoBuy(bool isBuy, Input input)
        {
            try
            {
                var marketid = $"{input.TargetCurrency}{input.BaseCurrency}";
                var baseCurrency = input.BaseCurrency;
                var targetCurrency = input.TargetCurrency;
                //var initalPrice = input.InitalPrice;
                var depth = input.ExchangeApi.GetOrderBook(marketid, 10);
                var txPrice =
                    input.InitalPrice == 0 ?
                    (isBuy ? depth.Asks.OrderBy(a => a.Value.Price).FirstOrDefault() : depth.Bids.OrderByDescending(a => a.Value.Price).FirstOrDefault()).Value.Price
                    : input.InitalPrice;

                //获取账户信息，确定目前账户存在多少钱和多少币
                var account = input.ExchangeApi.GetAmountsAvailableToTrade();
                var order = new ExchangeOrderRequest();
                if (isBuy)
                {
                    //可买的比特币量            
                    var amountTx = account.TryGetValueOrDefault(baseCurrency, 0) / txPrice;
                    if (amountTx < 0.001m) return Task.CompletedTask;
                    order.Amount = SelfMath.ToFixed(amountTx * decimal.Parse(input.Hold) / 100, int.Parse(input.Precision));
                    order.IsBuy = isBuy;
                    order.Price = txPrice;
                    order.Symbol = marketid;
                    input.ExchangeApi.PlaceOrder(order);
                    input.InitalPrice = order.Price;
                    input.Amount = order.Amount.ToString();
                    input.Status = InputStatus.Doing;
                }
                else
                {
                    var rangePriceMin = input.InitalPrice * (1 + decimal.Parse(input.GainPointMin) / 100);
                    //var rangePriceMax = holdPrice * (1 + decimal.Parse(TxtRangeMax.Text.Trim()));
                    var lossPrice = input.InitalPrice * (1 - decimal.Parse(input.LossPoint) / 100);
                    input.SellPrice = txPrice;
                    if ((txPrice >= rangePriceMin) || (txPrice <= lossPrice))
                    {
                        order.Amount = SelfMath.ToFixed(decimal.Parse(input.Amount) * (1 - 2.5m / 1000), int.Parse(input.Precision));
                        order.IsBuy = isBuy;
                        order.Price = txPrice;
                        order.Symbol = marketid;
                        input.ExchangeApi.PlaceOrder(order);
                        input.Status = InputStatus.Done;
                    }
                }
            }
            catch (Exception e)
            {
                using (FileStream fs = new FileStream("log.txt", FileMode.Append))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(e.Message.ToString() + "/r/n");
                    }
                }
            }
            return Task.CompletedTask;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var data = JsonConvert.SerializeObject(inputList);
            File.WriteAllText("data.json", data);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitSetting();
            timer.Start();
        }

        // 获取火币的总资产(折算为USDT)
        private decimal GetAmountUSDT()
        {
            var account = exchange.GetAmountsAvailableToTrade().ToList();
            var tasks = new Task<decimal>[account.Count];
            var ethUsdt = GetEthUSDT();
            Parallel.For(0, account.Count, i =>
            {
                tasks[i] = CalculateUSDT(account[i], ethUsdt);
            });
            var amounts = Task.WhenAll(tasks).GetAwaiter().GetResult();
            return amounts.Sum();
        }

        // 获取火币中ETH对应的交易币种
        private string[] GetETHToken()
        {
            string path = Application.StartupPath + "\\ETH.txt";
            string[] str1;
            using (StreamReader sr = new StreamReader(path, Encoding.UTF8, true))
            {
                str1 = sr.ReadLine().Split(',');
            }
            return str1;
        }

        // 获取火币中ETH/USDT的实时卖一价格
        private decimal GetEthUSDT()
        {
            var depth = exchange.GetOrderBook("eth/usdt", 1);
            var bids = depth.Bids.OrderByDescending(a => a.Value.Price);
            return bids.ElementAt(0).Value.Price;
        }

        // 获取火币中USDT对应的交易币种
        private string[] GetUSDTToken()
        {
            string path = Application.StartupPath + "\\USDT.txt";
            string[] str1;
            using (StreamReader sr = new StreamReader(path, Encoding.UTF8, true))
            {
                str1 = sr.ReadLine().Split(',');
            }
            return str1;
        }

        private IExchangeAPI InitExchange(Input input)
        {
            IExchangeAPI exchange;
            switch (input.WebSite)
            {
                case "币安":
                    exchange = new ExchangeBinanceAPI();
                    return exchange;
                case "FCoin":
                    exchange = new ExchangeFCoinAPI();
                    return exchange;
                case "火币":
                    exchange = new ExchangeHuobiAPI();
                    //           
                    return exchange;
                default:
                    return null;
            }
        }
        /// <summary>
        /// 使DataGridView的列自适应宽度
        /// </summary>
        /// <param name="dgViewFiles"></param>
        private void AutoSizeColumn(DataGridView dgViewFiles)
        {
            int width = 0;
            //使列自使用宽度
            //对于DataGridView的每一个列都调整
            for (int i = 0; i < dgViewFiles.Columns.Count; i++)
            {
                //将每一列都调整为自动适应模式
                dgViewFiles.AutoResizeColumn(i, DataGridViewAutoSizeColumnMode.AllCells);
                //记录整个DataGridView的宽度
                width += dgViewFiles.Columns[i].Width;
            }
            //判断调整后的宽度与原来设定的宽度的关系，如果是调整后的宽度大于原来设定的宽度，
            //则将DataGridView的列自动调整模式设置为显示的列即可，
            //如果是小于原来设定的宽度，将模式改为填充。
            if (width > dgViewFiles.Size.Width)
            {
                dgViewFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            }
            else
            {
                dgViewFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            //冻结某列 从左开始 0，1，2
            dgViewFiles.Columns[1].Frozen = true;
        }

        // 初始化
        // 1.加载火币API
        // 2.加载火币购买持币情况
        // 3.获取火币配置信息(USDT交易币集合、ETH交易币集合、ETH/USDT价格)
        // 4.当前账户的资产总数(折合为USDT)
        private void InitSetting()
        {
            // 1.加载火币API            
            for (int i = 0; i < splitContainer1.Panel1.Controls.Count; i++)
            {
                if (splitContainer1.Panel1.Controls[i] is ComboBox item)
                {
                    item.SelectedItem = item.Items[0];
                }
            }

            // 2.加载火币购买持币情况
            if (File.Exists("data.json"))
            {
                var data = File.ReadAllText("data.json");
                inputList = JsonConvert.DeserializeObject<List<Input>>(data);

                dataGrid.DataSource = null;
                dataGrid.DataSource = inputList;

                this.dataGrid.DataSource = this.TableAdapter(inputList);
                this.dataGrid.Columns[0].HeaderText = "持有币种";
                this.dataGrid.Columns[1].HeaderText = "止损率";
                this.dataGrid.Columns[2].HeaderText = "止盈率";
                this.dataGrid.Columns[3].HeaderText = "盈利率";
                this.dataGrid.Columns[4].HeaderText = "持仓比例";
                this.dataGrid.Columns[5].HeaderText = "运行情况";
                AutoSizeColumn(this.dataGrid);

            }
            // 3.获取火币配置信息(USDT交易币集合、ETH交易币集合、ETH / USDT价格)
            this.usdt = this.GetUSDTToken();
            this.eth = GetETHToken();
            this.init_usdt = this.GetAmountUSDT();
            this.textBox1.Text = this.init_usdt.ToString();
            this.BoxHold.SelectedItem = this.BoxHold.Items[1];
            // 4.当前账户的资产总数(折合为USDT)
            string path = Application.StartupPath + "\\账户信息.txt";
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(this.init_usdt);
                }
            }
        }
        private void timer_Tick(object sender, EventArgs e)
        {


        }
    }
    public class Input
    {
        public string BaseCurrency { get; set; }
        [JsonIgnore]
        public IExchangeAPI ExchangeApi { get; set; }
        public decimal Gain { get; set; }
        public string Hold { get; set; }
        public decimal InitalPrice { get; set; }
        public DateTime InitalTime { get; set; }
        //public string RangeTimeMax { get; set; }
        public string LossPoint { get; set; }
        public string GainPointMax { get; set; }
        public string GainPointMin { get; set; }
        public string RangeTimeMin { get; set; }
        public decimal SellPrice { get; set; }
        public InputStatus Status { get; set; }
        public string TargetCurrency { get; set; }
        public string WebSite { get; set; }
        public string Amount { get; set; }
        private string _Precision;
        public string Precision
        {
            get => string.IsNullOrEmpty(this._Precision) ? "2" : this._Precision;
            set => this._Precision = value;
        }

        public InputSrc InputSrc { get; set; }
    }

    public class InputValidator : AbstractValidator<Input>
    {
        public InputValidator()
        {
            RuleFor(i => i.BaseCurrency).NotNull().NotEmpty();
            RuleFor(i => i.Hold).Must(IsNumeric);
            RuleFor(i => i.LossPoint).Must(IsNumeric);
            RuleFor(i => i.GainPointMin).Must(IsNumeric);
            RuleFor(i => i.GainPointMax).Must(IsNumeric);
            //RuleFor(i => i.RangeTimeMax).Must(IsNumeric);
            RuleFor(i => i.RangeTimeMin).Must(IsNumeric);
            RuleFor(i => i.TargetCurrency).NotNull().NotEmpty();
            RuleFor(i => i.Status).NotNull();
            RuleFor(i => i.Precision.ToString()).Must(IsNumeric);
        }
        public bool IsNumeric(string value)
        {
            return (!string.IsNullOrEmpty(value)) && Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }
    }
}
