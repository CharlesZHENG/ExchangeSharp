using ExchangeSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using ExchangeSharp.API.Exchanges;

namespace BitKiwi
{
    class Program
    {
        static IExchangeAPI exchange = new ExchangeFCoinAPI();//new ExchangeHuobiAPI();
        private static decimal floatamountbuy;//累计买单深度
        private static decimal floatamountsell;//累计卖单深度
        private static decimal diffprice;//买卖价差
        private static int sleeptime;//睡眠时间
        private static string baseCurrency;//基币种
        private static string targetCurrency;//目标币种
        private static decimal floatPrice;//浮动价格
        private static decimal limitTargetAmount;//目标币止损位
        private static decimal limitBaseAmount;//基础币止损位
        private static decimal deep;//基础币止损位

        static void Main(string[] args)
        {
            //floatamountbuy = 2m;
            //floatamountsell = 2m;
            //diffprice = 0.0001m;//0.1%
            //floatPrice = 0.0001m;//0.1%
            //sleeptime = 5;
            //baseCurrency = "ht";
            //limitTargetAmount = 0.1m;
            //limitBaseAmount = 0.1m;
            //targetCurrency = "xrp";
            floatamountbuy = 1m;
            floatamountsell = 1m;
            diffprice = 0.00001m;//
            floatPrice = 0.00001m;
            sleeptime = 5;
            baseCurrency = "ht";
            limitTargetAmount = 1m;
            limitBaseAmount = 1m;
            targetCurrency = "eos";
            deep = 5;

            while (true)
            {
                Console.WriteLine($"{DateTime.Now}-开始:");
                OnTick(targetCurrency);
            }
        }
        static void CancelPendingOrders(string marketId)
        {
            var orders = exchange.GetOpenOrderDetails(marketId);
            foreach (var orderResult in orders)
            {
                exchange.CancelOrder(orderResult.OrderId, orderResult.Symbol);
                Console.WriteLine($"{DateTime.Now}-取消:{marketId}-{orderResult.OrderId}-{orderResult.Symbol}");
            }
        }
        //计算将要下单的价格
        public static decimal GetPrice(ExType type, string marketid, int count = 20)//20
        {
            var depth = exchange.GetOrderBook(marketid, count);
            var bids = depth.Bids.OrderByDescending(a => a.Value.Price);
            var asks = depth.Asks.OrderBy(a => a.Value.Price);
            decimal amountBids = 0;
            decimal amountAsks = 0;
            //计算买价，获取累计深度达到预设的价格
            if (type == ExType.Buy)
            {
                for (var i = 0; i < deep; i++)
                {
                    amountBids += bids.ElementAt(i).Value.Amount;
                    //floatamountbuy就是预设的累计买单深度
                    if (amountBids > floatamountbuy)
                    {
                        //稍微加0.01，使得订单排在前面
                        return bids.ElementAt(i).Value.Price + 1.10m * floatPrice;
                    }
                }
            }
            //同理计算卖价
            if (type == ExType.Sell)
            {
                for (var j = 0; j < deep; j++)
                {
                    amountAsks += asks.ElementAt(j).Value.Amount;
                    if (amountAsks > floatamountsell)
                    {
                        return asks.ElementAt(j).Value.Price - floatPrice;
                    }
                }
            }
            //遍历了全部深度仍未满足需求，就返回一个价格，以免出现bug
            return asks.ElementAt(0).Value.Price;
        }

        public static void OnTick(string currency)
        {
            var buyPrice = GetPrice(ExType.Buy, currency + baseCurrency);
            var sellPrice = GetPrice(ExType.Sell, currency + baseCurrency);
            //买卖价差如果小于预设值diffprice，就会挂一个相对更深的价格
            if ((sellPrice - buyPrice) <= diffprice)
            {
                buyPrice -= diffprice;
                sellPrice += diffprice;
            }
            //把原有的单子全部撤销，实际上经常出现新的价格和已挂单价格相同的情况，此时不需要撤销
            CancelPendingOrders(currency + baseCurrency);
            //获取账户信息，确定目前账户存在多少钱和多少币
            var account = exchange.GetAmountsAvailableToTrade();
            Console.WriteLine($"{DateTime.Now}-base账户余额{account.TryGetValueOrDefault(baseCurrency, 0)}");
            File.AppendAllTextAsync("C://Amountlog.txt", $"{DateTime.Now}-base账户余额{account.TryGetValueOrDefault(baseCurrency, 0)}\r\n");
            Console.WriteLine($"{DateTime.Now}-target账户余额{account.TryGetValueOrDefault(targetCurrency, 0)}");
            //可买的比特币量            
            var amountBuy = SelfMath.ToFixed((account.TryGetValueOrDefault(baseCurrency, 0) / buyPrice - 0.1m), 2);
            //可卖的比特币量，注意到没有仓位的限制，有多少就买卖多少，因为我当时的钱很少
            var amountSell = SelfMath.ToFixed(account.TryGetValueOrDefault(currency, 0), 2);
            if (amountSell > limitTargetAmount)
            {
                exchange.PlaceOrder(new ExchangeOrderRequest() { Amount = amountSell, IsBuy = false, Price = sellPrice, Symbol = currency + baseCurrency });
                Console.WriteLine($"{DateTime.Now}-卖单 数量:{amountSell}-价格:{sellPrice}");
            }
            if (amountBuy > limitBaseAmount)
            {
                exchange.PlaceOrder(new ExchangeOrderRequest() { Amount = amountBuy, IsBuy = true, Price = buyPrice, Symbol = currency + baseCurrency });
                Console.WriteLine($"{DateTime.Now}-买单 数量:{amountBuy}-价格:{buyPrice}");
            }
            //休眠，进入下一轮循环
            Thread.Sleep(sleeptime);
        }
    }
}