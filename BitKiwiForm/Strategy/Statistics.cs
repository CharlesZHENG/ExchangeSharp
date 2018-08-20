using Accord.Statistics;
using Accord.Statistics.Analysis;
using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BitKiwiForm.Strategy
{
    public class Statistics
    {
        public Data GetGata(string market, IExchangeAPI exchange)
        {
            return new Data { DateTime = DateTime.Now, Price = GetFirstAsk(market, exchange) };
        }
        public (bool IsNormal, double Max, double Min, double StdDev, double Mean) GetStatisticsResult(double[] data)
        {
            var analysis = new DistributionAnalysis();
            var gof = analysis.Learn(data);
            var fit = gof.Select<KeyValuePair<string, GoodnessOfFit>, GoodnessOfFit>(x => x.Value).ToArray();
            var isOk = false;
            var goFit = fit.OrderBy(x => x.ChiSquareRank).ToArray();
            for (int i = 0; i < 5; i++)
            {
                if (goFit[i]?.Name == "Normal") isOk = true;
            }
            return (isOk, data.Max(), data.Min(), data.StandardDeviation(), data.Mean());
        }
        private decimal GetFirstAsk(string market, IExchangeAPI exchange)
        {
            var depth = exchange.GetOrderBook(market, 1);
            var asks = depth.Asks.OrderByDescending(a => a.Value.Price);
            return asks.ElementAt(0).Value.Price;
        }
    }
    public class Data
    {
        public DateTime DateTime { get; set; }
        public decimal Price { get; set; }
    }
}
