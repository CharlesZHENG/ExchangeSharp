using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using Accord.Statistics.Analysis;

namespace GetWhiteList
{
    class Program
    {
        static IExchangeAPI exchange = new ExchangeHuobiAPI();
        public ObservableCollection<GoodnessOfFitViewModel> Analysis { get; private set; }
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
                StatisticsAnalyze(list.Select(a => (double)a.Price).ToArray());
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

        static bool StatisticsAnalyze(double[] list)
        {
            var analysis = new DistributionAnalysis();
            var gof = analysis.Learn(list);
            var fit = Enumerable.Select<KeyValuePair<string, GoodnessOfFit>, GoodnessOfFit>(gof, x => x.Value).ToArray();
            var first = fit.OrderBy(x => x.ChiSquareRank).FirstOrDefault();
            return first?.Name == "Normal";
        }
    }
    class Data
    {
        public DateTime DateTime { get; set; }
        public decimal Price { get; set; }
    }
    public class GoodnessOfFitViewModel
    {
        /// <summary>
        ///   Gets the rank of the goodness of fit test against
        ///   the target distribution in this good-of-fit test.
        /// </summary>
        /// 
        public string Rank { get; private set; }

        /// <summary>
        ///   Gets the name of the target distribution 
        ///   assessed by the goodness-of-fit test.
        /// </summary>
        /// 
        public string Name { get; private set; }

        /// <summary>
        ///   Gets the value of the Chi-Square goodness-of-fit test.
        /// </summary>
        /// 
        public double ChiSquare { get; private set; }

        /// <summary>
        ///   Gets the value of the Kolmogorov-Smirnov goodness-of-fit test.
        /// </summary>
        /// 
        public double KolmogorovSmirnov { get; private set; }

        /// <summary>
        ///   Initializes a new instance of the <see cref="GoodnessOfFitViewModel"/> class.
        /// </summary>
        /// 
        /// <param name="gof">The goodness-of-fits results against a particular distribution.</param>
        /// 
        public GoodnessOfFitViewModel(GoodnessOfFit gof)
        {
            this.Name = DistributionManager.Normalize(gof.Distribution.GetType().Name);

            // Transform the rank to ordinal positions
            // i.e. 0 to "1st", 1 to "2nd", 2 to "3rd"
            this.Rank = suffix(gof.ChiSquareRank + 1);

            this.ChiSquare = gof.ChiSquare;
            this.KolmogorovSmirnov = gof.KolmogorovSmirnov;
        }


        private static string suffix(int num)
        {
            if (num.ToString().EndsWith("11")) return num.ToString() + "th";
            if (num.ToString().EndsWith("12")) return num.ToString() + "th";
            if (num.ToString().EndsWith("13")) return num.ToString() + "th";
            if (num.ToString().EndsWith("1")) return num.ToString() + "st";
            if (num.ToString().EndsWith("2")) return num.ToString() + "nd";
            if (num.ToString().EndsWith("3")) return num.ToString() + "rd";
            return num.ToString() + "th";
        }
    }
}

