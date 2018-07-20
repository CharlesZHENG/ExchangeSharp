using ExchangeSharp;
using ExchangeSharp.API.Exchanges;
using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BitKiwiForm
{
    public partial class Form1 : Form
    {

        private decimal floatamountbuy;//累计买单深度
        private decimal floatamountsell;//累计卖单深度
        private decimal diffprice;//买卖价差
        private int sleeptime = 3000;//睡眠时间
        private List<Input> inputList = new List<Input>();
        //private static List<ViewModel> gridList = new List<ViewModel>();

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("data.json"))
            {
                var data = File.ReadAllText("data.json");
                inputList = JsonConvert.DeserializeObject<List<Input>>(data);
            }
            for (int i = 0; i < splitContainer1.Panel1.Controls.Count; i++)
            {
                if (splitContainer1.Panel1.Controls[i] is ComboBox item)
                {
                    item.SelectedItem = item.Items[0];
                }
            }
            this.BoxHold.SelectedItem = this.BoxHold.Items[1];
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var input = new Input()
            {
                BaseCurrency = BoxBaseCoin.SelectedItem.ToString().ToLower(),
                Hold = BoxHold.SelectedItem.ToString(),
                LossPoint = BoxLossPoint.SelectedItem.ToString(),
                RangePriceMin = BoxRangePrice.SelectedItem.ToString(),
                RangePriceMax = "120",
                RangeTimeMin = BoxRangeTime.SelectedItem.ToString(),
                //RangeTimeMax = TxtTimeMax.Text,
                TargetCurrency = TxtTargetCoin.Text.ToLower(),
                WebSite = Box.SelectedItem.ToString(),
                Status = InputStatus.None,
                InitalTime = DateTime.Now
            };
            var validator = new InputValidator();
            ValidationResult results = validator.Validate(input);
            if (results.IsValid)
            {
                input.ExchangeApi = InitExchange(input);
                inputList.Add(input);
                SetPrompInfo(inputList);
            }
            else
            {
                var error = results.Errors.Select(o => o.ErrorMessage).ToList();
                MessageBox.Show($"输入参数有误{JsonConvert.SerializeObject(error)}");
            }
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
                //可买的比特币量            
                var amountTx = SelfMath.ToFixed((account.TryGetValueOrDefault((isBuy ? baseCurrency : targetCurrency), 0) / txPrice), 2);
                if (amountTx < 0.1m) return Task.CompletedTask;
                var order = new ExchangeOrderRequest();
                if (isBuy)
                {
                    //优化
                    order.Amount = amountTx * decimal.Parse(input.Hold) / 100;
                    order.IsBuy = isBuy;
                    order.Price = txPrice;
                    order.Symbol = marketid;
                    input.ExchangeApi.PlaceOrder(order);
                    input.InitalPrice = order.Price;
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
                        order.Amount = amountTx;
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
            }
            return Task.CompletedTask;
        }
        public delegate void DelegateSetCotnent(List<Input> list);
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
                            inputList[i].ExchangeApi = InitExchange(inputList[i]);
                            tasks[i] = DoBuy(false, inputList[i]);
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
                    SetPrompInfo(inputList);

                    if (inputList.All(a => a.Status == InputStatus.Done))
                    {
                        break;
                    }
                    Thread.Sleep(sleeptime);
                }
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var data = JsonConvert.SerializeObject(inputList);
            File.WriteAllText("data.json", data);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGrid.SelectedRows.Count>0)
            {
                var index = dataGrid.CurrentRow.Index;
                inputList.RemoveAt(index);
                dataGrid.DataSource = null;
                dataGrid.DataSource = inputList;
            }
        }
        private void BtnSave_Click(object sender, EventArgs e)
        {
            var data = JsonConvert.SerializeObject(inputList);
            File.WriteAllText("data.json", data);
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
        public string WebSite { get; set; }
        public string TargetCurrency { get; set; }
        public string BaseCurrency { get; set; }
        public string RangePriceMin { get; set; }
        public string RangePriceMax { get; set; }
        public string RangeTimeMin { get; set; }
        //public string RangeTimeMax { get; set; }
        public string LossPoint { get; set; }
        public string Hold { get; set; }
        public DateTime InitalTime { get; set; }
        public decimal InitalPrice { get; set; }
        public InputStatus Status { get; set; }
        [JsonIgnore]
        public IExchangeAPI ExchangeApi { get; set; }
        public decimal SellPrice { get; set; }
        public decimal Gain { get; set; }
    }
    public enum InputStatus
    {
        None = 0,
        Doing = 1,
        Done = 2
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
