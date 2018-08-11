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

namespace BitKiwiForm
{
    public enum InputStatus
    {
        None = 0,
        Doing = 1,
        Done = 2
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
                row.SellPoint = input.RangePriceMin.ToString();
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
            input.RangePriceMin = BoxRangePrice.Text;
            input.RangePriceMax = "120";
            input.RangeTimeMin = BoxRangeTime.SelectedItem.ToString();
            //input.RangeTimeMax = TxtTimeMax.Text;
            input.TargetCurrency = TxtTargetCoin.Text.ToLower();
            input.WebSite = Box.SelectedItem.ToString();
            input.Status = InputStatus.None;
            input.InitalTime = DateTime.Now;
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
            }
        }

        private void BtnGo_Click(object sender, EventArgs e)
        {
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
                var price = isBuy ? depth.Asks.OrderBy(a => a.Value.Price).FirstOrDefault() : depth.Bids.OrderByDescending(a => a.Value.Price).FirstOrDefault();
                var txPrice = price.Value.Price;
                //获取账户信息，确定目前账户存在多少钱和多少币
                var account = input.ExchangeApi.GetAmountsAvailableToTrade();
                var order = new ExchangeOrderRequest();
                if (isBuy)
                {
                    //可买的比特币量            
                    var amountTx = account.TryGetValueOrDefault(baseCurrency, 0) / txPrice;
                    if (amountTx < 0.001m) return Task.CompletedTask;
                    order.Amount = SelfMath.ToFixed(amountTx * decimal.Parse(input.Hold) / 100, 2);
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
                    var rangePriceMin = input.InitalPrice * (1 + decimal.Parse(input.RangePriceMin) / 100);
                    //var rangePriceMax = holdPrice * (1 + decimal.Parse(TxtRangeMax.Text.Trim()));
                    var lossPrice = input.InitalPrice * (1 - decimal.Parse(input.LossPoint) / 100);
                    input.SellPrice = txPrice;
                    if ((txPrice >= rangePriceMin) || (txPrice <= lossPrice))
                    {
                        order.Amount = decimal.Parse(input.Amount) * (1 - 2 / 1000);
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

            //return amounts.Sum();
            //var account = exchange.GetAmountsAvailableToTrade();
            //decimal amount = 0;           
            //foreach (var account1 in account)
            //{
            //    // 如果持有币种小于0.01，不纳入统计
            //    if (account1.Value<0.01m)
            //    {

            //    }
            //    // 如果持有币种为usdt
            //    else if ("usdt".Equals(account1.Key))
            //    {
            //        amount += account1.Value;
            //    }                
            //    else
            //    {
            //        // 如果该币在usdt交易区，则折算为usdt
            //        if (this.usdt.Contains(account1.Key.ToUpper()))
            //        {
            //            var depth = exchange.GetOrderBook(account1.Key + "/usdt", 1);
            //            var bids = depth.Bids.OrderByDescending(a => a.Value.Price);
            //            amount += account1.Value * bids.ElementAt(0).Value.Price;
            //        }
            //        // 如果该币在eth交易区，则先折算为eth，再折算为usdt
            //        else if(this.eth.Contains(account1.Key.ToUpper()))
            //        {                       
            //            var depth = exchange.GetOrderBook(account1.Key + "/eth", 1);
            //            var bids = depth.Bids.OrderByDescending(a => a.Value.Price);
            //            amount += account1.Value* bids.ElementAt(0).Value.Price* this.GetEthUSDT();
            //        }
            //    }
            //}            
            //return amount;
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
            DateTime dt1 = System.DateTime.Now;
            var depth = exchange.GetOrderBook("eth/usdt", 1);
            DateTime dt2 = System.DateTime.Now;
            TimeSpan ts = dt2.Subtract(dt1);
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
                    return exchange;
                default:
                    return null;
            }
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
            Task.Run(() =>
            {
                SetAmount(this.GetAmountUSDT().ToString(), (this.GetAmountUSDT() - this.init_usdt).ToString());
            });
        }
    }
    /*
    public class ViewModel
    {
        public string TargetCurrency { get; set; }
        public string BaseCurrency { get; set; }
        public decimal LossPoint { get; set; }
        public decimal Hold { get; set; }
        public DateTime InitalTime { get; set; }
        public decimal InitalPrice { get; set; }
        public decimal RangePriceMin { get; set; }
        public decimal RangePriceMax { get; set; }
        public decimal RangeTimeMin { get; set; }
        public decimal SellPrice { get; set; }
        public decimal Gain { get; set; }
        public InputStatus Status { get; set; }

        public static ViewModel From(Input input)
        {
            return new ViewModel()
            {
                TargetCurrency = input.TargetCurrency,
                BaseCurrency = input.BaseCurrency,
                Gain = (input.SellPrice - input.InitalPrice) / input.InitalPrice * 100,
            };
        }
    }
    */

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
        public string RangePriceMax { get; set; }
        public string RangePriceMin { get; set; }
        public string RangeTimeMin { get; set; }
        public decimal SellPrice { get; set; }
        public InputStatus Status { get; set; }
        public string TargetCurrency { get; set; }
        public string WebSite { get; set; }
        public string Amount { get; set; }
    }
    public class InputValidator : AbstractValidator<Input>
    {
        public InputValidator()
        {
            RuleFor(i => i.BaseCurrency).NotNull().NotEmpty();
            RuleFor(i => i.Hold).Must(IsNumeric);
            RuleFor(i => i.LossPoint).Must(IsNumeric);
            RuleFor(i => i.RangePriceMin).Must(IsNumeric);
            RuleFor(i => i.RangePriceMax).Must(IsNumeric);
            //RuleFor(i => i.RangeTimeMax).Must(IsNumeric);
            RuleFor(i => i.RangeTimeMin).Must(IsNumeric);
            RuleFor(i => i.TargetCurrency).NotNull().NotEmpty();
            RuleFor(i => i.Status).NotNull();
        }
        public bool IsNumeric(string value)
        {
            return (!string.IsNullOrEmpty(value)) && Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }
    }
}
