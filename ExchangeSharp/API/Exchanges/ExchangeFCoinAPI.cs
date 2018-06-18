using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ExchangeSharp.API.Exchanges
{
    public sealed class ExchangeFCoinAPI : ExchangeAPI
    {
        public override string BaseUrl { get; set; } = "https://api.fcoin.com/v2/";
        public override string Name => ExchangeName.FCoin;
        private string sig;
        private string timestamp;

        public ExchangeFCoinAPI()
        {
            RequestContentType = "application/x-www-form-urlencoded";
            NonceStyle = NonceStyle.UnixSecondsString; // not used, see below
            SymbolSeparator = string.Empty;
            SymbolIsUppercase = false;
            //SymbolIsReversed = true;
        }

        public override string NormalizeSymbol(string symbol)
        {
            return (symbol ?? string.Empty).Replace("-", string.Empty).Replace("/", string.Empty)
                .Replace("_", string.Empty);
        }

        protected override async Task ProcessRequestAsync(HttpWebRequest request, Dictionary<string, object> payload)
        {
            request.Headers = new WebHeaderCollection()
            {
                {"FC-ACCESS-KEY", PublicApiKey.ToUnsecureString()},
                {"FC-ACCESS-SIGNATURE", sig},
                {"FC-ACCESS-TIMESTAMP", timestamp}
            };

            if (CanMakeAuthenticatedRequest(payload))
            {
                if (request.Method == "POST")
                {
                    //request.ContentType = "application/json";
                    payload.Remove("nonce");
                    var msg = CryptoUtility.GetJsonForPayload(payload);
                    await CryptoUtility.WriteToRequestAsync(request, msg);
                }
            }
        }

        protected override Uri ProcessRequestUrl(UriBuilder url, Dictionary<string, object> payload, string method)
        {
            timestamp = DateTime.UtcNow.ToString("s");
            if (CanMakeAuthenticatedRequest(payload))
            {
                if (!payload.ContainsKey("method"))
                {
                    return url.Uri;
                }

                method = payload["method"].ToStringInvariant();
                payload.Remove("method");
                string msg = CryptoUtility.GetFormForPayload(payload, false);
                msg = string.Join("&", new SortedSet<string>(msg.Split('&'), StringComparer.Ordinal));
                StringBuilder sb = new StringBuilder();
                sb.Append(method).Append("\n")
                    .Append(url.Host).Append("\n")
                    .Append(url.Path).Append("\n")
                    .Append(timestamp).Append("\n")
                    .Append(msg);
                sig = CryptoUtility.SHA1SignBase64(sb.ToString(), PrivateApiKey.ToBytes());
                url.Query = sb.ToString();
            }

            return url.Uri;
        }

        //***********Start*************

        protected override async Task<IEnumerable<string>> OnGetSymbolsAsync()
        {
            var m = await GetSymbolsMetadataAsync();
            return m.Select(x => x.MarketName);
        }

        protected override async Task<IEnumerable<ExchangeMarket>> OnGetSymbolsMetadataAsync()
        {
//            {
//                "status": 0,
//                "data": [
//                {
//                    "name": "btcusdt",
//                    "base_currency": "btc",
//                    "quote_currency": "usdt",
//                    "price_decimal": 2,
//                    "amount_decimal": 4
//                },
//                {
//                    "name": "ethusdt",
//                    "base_currency": "eth",
//                    "quote_currency": "usdt",
//                    "price_decimal": 2,
//                    "amount_decimal": 4
//                }
//                ]
//            }
            if (ReadCache("GetSymbolsMetadata", out List<ExchangeMarket> markets))
            {
                return markets;
            }

            markets = new List<ExchangeMarket>();
            JToken allSymbols = await MakeJsonRequestAsync<JToken>("public/symbols", BaseUrl, null);
            foreach (var symbol in allSymbols)
            {
                var marketCurrency = symbol["base_currency"].ToStringLowerInvariant();
                var baseCurrency = symbol["quote_currency"].ToStringLowerInvariant();
                var price_precision = symbol["price_decimal"].ConvertInvariant<double>();
                var priceStepSize = Math.Pow(10, -price_precision);
                var amount_precision = symbol["amount_decimal"].ConvertInvariant<double>();
                var quantityStepSize = Math.Pow(10, -amount_precision);

                var market = new ExchangeMarket()
                {
                    MarketCurrency = marketCurrency,
                    BaseCurrency = baseCurrency,
                    MarketName = marketCurrency + baseCurrency,
                    IsActive = true,
                };

                market.PriceStepSize = priceStepSize.ConvertInvariant<decimal>();
                market.QuantityStepSize = quantityStepSize.ConvertInvariant<decimal>();
                market.MinPrice = market.PriceStepSize.Value;
                market.MinTradeSize = market.QuantityStepSize.Value;

                markets.Add(market);
            }

            WriteCache("GetSymbolsMetadata", TimeSpan.FromMinutes(60.0), markets);

            return markets;
        }

        protected override async Task<Dictionary<string, decimal>> OnGetAmountsAsync()
        {
//            {
//                "status": 0,
//                "data": [
//                {
//                    "currency": "btc",
//                    "available": "50.0",
//                    "frozen": "50.0",
//                    "balance": "100.0"
//                }
//                ]
//            }
            Dictionary<string, decimal> amounts = new Dictionary<string, decimal>();
            JToken token = await MakeJsonRequestAsync<JToken>($"accounts/balance", BaseUrl, null);
            var list = token["data"];
            foreach (var item in list)
            {
                var balance = item["balance"].ConvertInvariant<decimal>();
                if (balance == 0m)
                    continue;

                var currency = item["currency"].ToStringInvariant();

                if (amounts.ContainsKey(currency))
                {
                    amounts[currency] += balance;
                }
                else
                {
                    amounts[currency] = balance;
                }
            }

            return amounts;
        }

        protected override async Task<Dictionary<string, decimal>> OnGetAmountsAvailableToTradeAsync()
        {
            Dictionary<string, decimal> amounts = new Dictionary<string, decimal>();
            JToken token = await MakeJsonRequestAsync<JToken>($"accounts/balance", BaseUrl);
            var list = token["data"];
            foreach (var item in list)
            {
                var balance = item["balance"].ConvertInvariant<decimal>();
                if (balance == 0m)
                    continue;
                var currency = item["available"].ToStringInvariant();

                if (amounts.ContainsKey(currency))
                {
                    amounts[currency] += balance;
                }
                else
                {
                    amounts[currency] = balance;
                }
            }

            return amounts;
        }

        protected override async Task OnCancelOrderAsync(string orderId, string symbol = null)
        {
            var payload = await OnGetNoncePayloadAsync();
            payload["method"] = "POST";
            await MakeJsonRequestAsync<JToken>($"orders/{orderId}/submit-cancel", BaseUrl, payload, "POST");
        }

        protected override async Task<ExchangeOrderBook> OnGetOrderBookAsync(string symbol, int maxCount = 100)
        {
            string level = maxCount == 20 ? "L20" : maxCount == 100 ? "L100" : "full";
            symbol = NormalizeSymbol(symbol);
            ExchangeOrderBook orders = new ExchangeOrderBook();
            JToken obj = await MakeJsonRequestAsync<JToken>($"market/depth/{level}/{symbol}", BaseUrl, null);
            return ExchangeAPIExtensions.ParseOrderBookFromJTokenArrays(obj["tick"], sequence: "ts",
                maxCount: maxCount);
        }

        //下单
        protected override async Task<ExchangeOrderResult> OnPlaceOrderAsync(ExchangeOrderRequest order)
        {
            Dictionary<string, object> payload = new Dictionary<string, object>();
            payload.Add("amount	",order.Amount.ToString());
            payload.Add("method	","POST");
            payload.Add("price",order.Price.ToString());
            payload.Add("side",order.IsBuy?"bug":"sell");
            payload.Add("symbol",NormalizeSymbol(order.Symbol));
            payload.Add("type",order.OrderType.ToString());
            JToken obj= await MakeJsonRequestAsync<JToken>($"orders", BaseUrl, payload, "POST");
            ExchangeOrderResult result=new ExchangeOrderResult()
            {
                Amount = order.Amount,
                Price = order.Price,
                IsBuy = order.IsBuy,
                OrderId = obj["data"].ToStringInvariant(),
                Symbol = order.Symbol
            };
            result.AveragePrice = result.Price;
            result.Result = ExchangeAPIOrderResult.Pending;

            return result;
        }

//*************End**********

        protected override async Task<IEnumerable<ExchangeOrderResult>> OnGetOpenOrderDetailsAsync(string symbol = null)
        {
            if (symbol == null)
            {
                throw new APIException("symbol cannot be null");
            }

            List<ExchangeOrderResult> orders = new List<ExchangeOrderResult>();
            var payload = await OnGetNoncePayloadAsync();
            payload["method"] = "GET";
            payload.Add("symbol", symbol);
            //payload.Add("states", "pre-submitted,submitting,submitted,partial-filled");
            JToken data = await MakeJsonRequestAsync<JToken>("/orders", BaseUrl, payload);
            foreach (var prop in data)
            {
                orders.Add(ParseOrder(prop));
            }

            return orders;
        }

        private ExchangeOrderResult ParseOrder(JToken token)
        {
            ExchangeOrderResult result = new ExchangeOrderResult()
            {
                //OrderId = token["id"].ToStringInvariant(),
                //Symbol = token["symbol"].ToStringInvariant(),
                //Amount = token["amount"].ConvertInvariant<decimal>(),
                //AmountFilled = token["field-amount"].ConvertInvariant<decimal>(),
                //Price = token["price"].ConvertInvariant<decimal>(),
                //OrderDate = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(token["created-at"].ConvertInvariant<long>()),
                //IsBuy = token["type"].ToStringInvariant().StartsWith("buy"),
                //Result = ParseState(token["state"].ToStringInvariant()),
            };
            return result;
        }

        private ExchangeAPIOrderResult ParseState(string state)
        {
            switch (state)
            {
                //case "pre-submitted":
                //case "submitting":
                //case "submitted":
                //    return ExchangeAPIOrderResult.Pending;
                //case "partial-filled":
                //    return ExchangeAPIOrderResult.FilledPartially;
                //case "filled":
                //    return ExchangeAPIOrderResult.Filled;
                //case "partial-canceled":
                //case "canceled":
                //    return ExchangeAPIOrderResult.Canceled;
                default:
                    return ExchangeAPIOrderResult.Unknown;
            }
        }

        protected override async Task<Dictionary<string, object>> OnGetNoncePayloadAsync()
        {
            var result = await base.OnGetNoncePayloadAsync();
            result["method"] = "POST";
            return result;
        }
    }
}