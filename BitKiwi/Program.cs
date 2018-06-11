using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExchangeSharp;

namespace BitKiwi
{
    class Program
    {
        static IExchangeAPI exchange = new ExchangeHuobiAPI();
        private static decimal floatamountbuy;//累计买单深度
        private static decimal floatamountsell;//累计卖单深度
        private static decimal diffprice;//买卖价差
        private static int sleeptime;//睡眠时间
        private static string baseCurrency;//基币种

        static void Main(string[] args)
        {
            exchange.LoadAPIKeysUnsecure("", "");
            
            floatamountbuy = 0m;
            floatamountsell = 0m;
            diffprice = 0m;
            sleeptime = 0;
            baseCurrency = "ht";

            while (true)
            {
                OnTick("btc");
            }
        }
        static void CancelPendingOrders(string currency)
        {
            var orders = exchange.GetOpenOrderDetails(currency);
            foreach (var orderResult in orders)
            {
                exchange.CancelOrder(orderResult.OrderId, orderResult.Symbol);
            }
        }
        //计算将要下单的价格
        public static decimal GetPrice(ExType type, string currency, int count = 20)//20
        {
            var depth = exchange.GetOrderBook(currency, count);
            decimal amountBids = 0;
            decimal amountAsks = 0;
            //计算买价，获取累计深度达到预设的价格
            if (type == ExType.Buy)
            {
                for (var i = 0; i < 20; i++)
                {
                    amountBids += depth.Bids[i].Amount;
                    //floatamountbuy就是预设的累计买单深度
                    if (amountBids > floatamountbuy)
                    {
                        //稍微加0.01，使得订单排在前面
                        return depth.Bids[i].Price + 0.01m;
                    }
                }
            }
            //同理计算卖价
            if (type == ExType.Sell)
            {
                for (var j = 0; j < 20; j++)
                {
                    amountAsks += depth.Asks[j].Amount;
                    if (amountAsks > floatamountsell)
                    {
                        return depth.Asks[j].Price - 0.01m;
                    }
                }
            }
            //遍历了全部深度仍未满足需求，就返回一个价格，以免出现bug
            return depth.Asks[0].Price;
        }

        public static void OnTick(string currency)
        {
            var buyPrice = GetPrice(ExType.Buy, currency);
            var sellPrice = GetPrice(ExType.Sell, currency);
            //买卖价差如果小于预设值diffprice，就会挂一个相对更深的价格
            if ((sellPrice - buyPrice) <= diffprice)
            {
                buyPrice -= 10;
                sellPrice += 10;
            }
            //把原有的单子全部撤销，实际上经常出现新的价格和已挂单价格相同的情况，此时不需要撤销
            CancelPendingOrders(currency);
            //获取账户信息，确定目前账户存在多少钱和多少币
            var account = exchange.GetAmountsAvailableToTrade();
            //可买的比特币量
            var amountBuy = SelfMath.ToFixed((account[baseCurrency] / buyPrice - 0.1m), 2);
            //可卖的比特币量，注意到没有仓位的限制，有多少就买卖多少，因为我当时的钱很少
            var amountSell = SelfMath.ToFixed(account[currency], 2);
            if (amountSell > 0.02m)
            {
                exchange.PlaceOrder(new ExchangeOrderRequest() { Amount = amountSell, IsBuy = false, Price = sellPrice });
            }
            if (amountBuy > 0.02m)
            {
                exchange.PlaceOrder(new ExchangeOrderRequest() { Amount = amountBuy, IsBuy = true, Price = buyPrice });
            }
            //休眠，进入下一轮循环
            Thread.Sleep(sleeptime);
        }
    }
}
