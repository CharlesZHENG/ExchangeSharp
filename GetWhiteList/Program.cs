using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace GetWhiteList
{
    class Program
    {
        static IExchangeAPI exchange = new ExchangeHuobiAPI();
        static void Main(string[] args)
        {
            //币种-时间-价格
            //众数、Max、Min、Avg，概率模型
            exchange.LoadAPIKeysUnsecure("f87dde0a-cd2be878-dcfdea13-b252a", "e6f9ba8b-6e9183b5-784e0203-509bf");
            int sleepTime = 10 * 1000;
            string market = "ethusdt";
            int line = 1 * 10000;
            List<Data> list = new List<Data>();
            int count = 0;
            while (true)
            {
                list.Add(new Data { DateTime = DateTime.Now, Price = GetFirstAsk(market) });
                count++;
                if (list.Count == 1000)
                {
                    string path = $"{ (count / line) + 1}_data.csv";                    
                    File.AppendAllLines(path, list.Select(d => $"{d.DateTime},{d.Price}").ToArray());
                    list.Clear();
                }
                Thread.Sleep(sleepTime);
            }
        }
        static decimal GetFirstAsk(string market)
        {
            var depth = exchange.GetOrderBook(market, 1);
            var asks = depth.Asks.OrderByDescending(a => a.Value.Price);
            return asks.ElementAt(0).Value.Price;
        }
    }
    class Data
    {
        public DateTime DateTime { get; set; }
        public decimal Price { get; set; }
    }
}

