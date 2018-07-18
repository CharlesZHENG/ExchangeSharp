using ExchangeSharp;
using ExchangeSharp.API.Exchanges;
using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BitKiwiForm
{
    public partial class Form1 : Form
    {
        static IExchangeAPI exchange = new ExchangeBinanceAPI();
        private decimal floatamountbuy;//累计买单深度
        private decimal floatamountsell;//累计卖单深度
        private decimal diffprice;//买卖价差
        private int sleeptime;//睡眠时间
        private string baseCurrency;//基币种
        private string marketid;
        private string targetCurrency;
        private decimal holdPrice;
        private bool isStop;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Box.SelectedItem = Box.Items[0];
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
                RuleFor(i => i.RangeTimeMax).Must(IsNumeric);
                RuleFor(i => i.RangeTimeMin).Must(IsNumeric);
                RuleFor(i => i.TargetCurrency).NotNull().NotEmpty();
            }
            public bool IsNumeric(string value)
            {
                return (!string.IsNullOrEmpty(value)) && Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
            }
        }

        private void BtnSet_Click(object sender, EventArgs e)
        {
            var input = new Input()
            {
                BaseCurrency = TxtBaseCoin.Text,
                Hold = TxtHold.Text,
                LossPoint = TxtHold.Text,
                RangePriceMin = TxtRangeMin.Text,
                RangePriceMax = TxtRangeMax.Text,
                RangeTimeMin = TxtTimeMin.Text,
                RangeTimeMax = TxtTimeMax.Text,
                TargetCurrency = TxtTargetCoin.Text,
                WebSite = Box.SelectedText
            };
            var validator = new InputValidator();
            ValidationResult results = validator.Validate(input);

            if (results.IsValid)
            {
                InitExchange();
                targetCurrency = TxtTargetCoin.Text.Trim();
                baseCurrency = TxtBaseCoin.Text.Trim();
                marketid = $"{targetCurrency}{baseCurrency}";
                Task.Run(() =>
                {
                    DoBuy(true);
                    while (true)
                    {
                        Console.WriteLine($"{DateTime.Now}-开始:");
                        OnTick(targetCurrency);
                        if (isStop) break;
                    }
                });
            }
            else
            {
                var error = results.Errors.Select(o => o.ErrorMessage).ToList();
                MessageBox.Show($"输入参数有误{JsonConvert.SerializeObject(error)}");
            }
        }

        private void InitExchange()
        {
            var ex = Box.SelectedItem.ToString();
            switch (ex)
            {
                case "币安":
                    exchange = new ExchangeBinanceAPI();

                    break;
                case "FCoin":
                    exchange = new ExchangeFCoinAPI();
                    exchange.LoadAPIKeysUnsecure("0d30c4db02ef48009a61e8b179454d49", "e6585930d35640cd98eb6ddfa6de4e44");
                    break;
            }
        }

        public void OnTick(string currency)
        {
            //Console.WriteLine($"{DateTime.Now}-卖单 数量:{amountSell}-价格:{sellPrice}");
            DoBuy(false);
            //休眠，进入下一轮循环
            Thread.Sleep(sleeptime);
        }

        private void DoBuy(bool isBuy)
        {
            var depth = exchange.GetOrderBook(marketid, 10);
            var price = isBuy ? depth.Asks.OrderBy(a => a.Value.Price).FirstOrDefault() : depth.Bids.OrderByDescending(a => a.Value.Price).FirstOrDefault();
            var txPrice = price.Value.Price;
            //获取账户信息，确定目前账户存在多少钱和多少币
            var account = exchange.GetAmountsAvailableToTrade();
            //可买的比特币量            
            var amountTx = SelfMath.ToFixed((account.TryGetValueOrDefault((isBuy ? baseCurrency : targetCurrency), 0) / txPrice), 2);
            var order = new ExchangeOrderRequest();
            if (isBuy)
            {
                order.Amount = amountTx * decimal.Parse(TxtHold.Text.Trim()) / 100;
                order.IsBuy = isBuy;
                order.Price = txPrice;
                order.Symbol = marketid;
                exchange.PlaceOrder(order);
                holdPrice = order.Price;
            }
            else
            {
                var rangePriceMin = holdPrice * (1 + decimal.Parse(TxtRangeMin.Text.Trim()));
                var rangePriceMax = holdPrice * (1 + decimal.Parse(TxtRangeMax.Text.Trim()));
                var lossPrice = holdPrice * (1 - decimal.Parse(TxtLossPoint.Text.Trim()));
                if ((txPrice >= rangePriceMin && txPrice <= rangePriceMax) || (txPrice <= lossPrice))
                {
                    order.Amount = amountTx;
                    order.IsBuy = isBuy;
                    order.Price = txPrice;
                    order.Symbol = marketid;
                    exchange.PlaceOrder(order);
                    isStop = true;
                }
            }
        }
    }

    public class Input
    {
        public string WebSite { get; set; }
        public string TargetCurrency { get; set; }
        public string BaseCurrency { get; set; }
        public string RangePriceMin { get; set; }
        public string RangePriceMax { get; set; }
        public string RangeTimeMin { get; set; }
        public string RangeTimeMax { get; set; }
        public string LossPoint { get; set; }
        public string Hold { get; set; }
    }
}
