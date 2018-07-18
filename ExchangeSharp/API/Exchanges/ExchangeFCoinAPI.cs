using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ExchangeSharp.Utility;
using Newtonsoft.Json.Linq;

namespace ExchangeSharp.API.Exchanges
{
    public sealed class ExchangeFCoinAPI : ExchangeAPI
    {

        public override string BaseUrl { get; set; } = "https://api.fcoin.com/v2";
        public override string Name => ExchangeName.FCoin;

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
            var timestamp = TimeHelper.GetTotalMilliseconds().ToString();
            payload?.Remove("method");
            payload?.Remove("nonce");
            string msg = CryptoUtility.GetFormForPayload(payload, false);
            msg = string.Join("&", new SortedSet<string>(msg.Split('&'), StringComparer.Ordinal));
            //var msg = payload?.Count > 0 ? string.Join("&", new SortedSet<string>(payload.Select(a => $"{a.Key}={a.Value}").ToList())) : "";
            string signStr = (request.Method == "GET") ? $"{request.Method}{request.Address}{timestamp}" : $"{request.Method}{request.Address.AbsoluteUri}{timestamp}{msg}";
            Debug.WriteLine(signStr);
            var sig = CryptoUtility.SHA1SignBase64(signStr, PrivateApiKey.ToBytes());
            request.Headers = new WebHeaderCollection()
            {
                {"FC-ACCESS-KEY", PublicApiKey.ToUnsecureString()},
                {"FC-ACCESS-SIGNATURE", sig},
                {"FC-ACCESS-TIMESTAMP", timestamp}
            };

            if (request.Method == "POST")
            {
                request.ContentType = "application/json";
                msg = CryptoUtility.GetJsonForPayload(payload);
                await CryptoUtility.WriteToRequestAsync(request, msg);
            }
            else
            {
                request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            }
        }
        protected override Uri ProcessRequestUrl(UriBuilder url, Dictionary<string, object> payload, string method)
        {
            if (method == "GET")
            {
                payload?.Remove("method");
                string msg = CryptoUtility.GetFormForPayload(payload, false);
                url.Query = msg;
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
            //var ts = MakeJsonRequestAsync<JToken>("public/server-time", BaseUrl, null).GetAwaiter().GetResult();
            //timestamp = ts.ToStringInvariant();
            JToken token = await MakeJsonRequestAsync<JToken>($"accounts/balance", BaseUrl, null);
            foreach (var item in token)
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
            /*
            {
                "status": 0,
                "data": [
                {
                    "currency": "btc",
                    "available": "50.0",
                    "frozen": "50.0",
                    "balance": "100.0"
                }
                ]
            }*/
            Dictionary<string, decimal> amounts = new Dictionary<string, decimal>();
            var payload = await OnGetNoncePayloadAsync();
            payload["method"] = "GET";
            JToken token = await MakeJsonRequestAsync<JToken>($"accounts/balance", BaseUrl, payload, "GET");
            foreach (var item in token)
            {
                var balance = item["available"].ConvertInvariant<decimal>();
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
        protected override async Task OnCancelOrderAsync(string orderId, string symbol = null)
        {
            var payload = await OnGetNoncePayloadAsync();
            payload["method"] = "POST";
            await MakeJsonRequestAsync<JToken>($"orders/{orderId}/submit-cancel", BaseUrl, payload, "POST");
        }
        public async Task<ExchangeOrderBook> GetFilledOrdersAsync(string symbol, int maxCount = 100)
        {
            /*
         {
"type": "depth.L20.ethbtc",
"ts": 1523619211000,
"seq": 120,
"bids": [0.000100000, 1.000000000, 0.000010000, 1.000000000],
"asks": [1.000000000, 1.000000000]
}
         */
            string level = maxCount == 20 ? "L20" : maxCount == 100 ? "L100" : "full";
            symbol = NormalizeSymbol(symbol);
            var payload = await OnGetNoncePayloadAsync();
            payload["method"] = "GET";
            payload["states"] = "filled";
            JToken obj = await MakeJsonRequestAsync<JToken>($"market/depth/{level}/{symbol}", BaseUrl, payload, "GET");
            return ParseOrderBookFromJToken(obj, sequence: "ts",
                maxCount: maxCount);
        }

        protected override async Task<ExchangeOrderBook> OnGetOrderBookAsync(string symbol, int maxCount = 100)
        {
            /*
             {
  "type": "depth.L20.ethbtc",
  "ts": 1523619211000,
  "seq": 120,
  "bids": [0.000100000, 1.000000000, 0.000010000, 1.000000000],
  "asks": [1.000000000, 1.000000000]
}
             */
            string level = maxCount == 20 ? "L20" : maxCount == 100 ? "L100" : "full";
            symbol = NormalizeSymbol(symbol);
            var payload = await OnGetNoncePayloadAsync();
            payload["method"] = "GET";
            JToken obj = await MakeJsonRequestAsync<JToken>($"market/depth/{level}/{symbol}", BaseUrl, payload, "GET");
            return ParseOrderBookFromJToken(obj, sequence: "ts",
                maxCount: maxCount);
        }

        private ExchangeOrderBook ParseOrderBookFromJToken(JToken token, string asks = "asks", string bids = "bids", string price = "price", string amount = "amount", string sequence = "ts", int maxCount = 100)
        {
            ExchangeOrderBook book = new ExchangeOrderBook
            {
                SequenceId = token[sequence].ConvertInvariant<long>()
            };
            for (int i = 0; i < token[asks].Count() - 1; i = i + 2)
            {
                var depth = new ExchangeOrderPrice { Price = token[asks][i].ConvertInvariant<decimal>(), Amount = token[asks][i + 1].ConvertInvariant<decimal>() };
                book.Asks[depth.Price] = depth;
                if (book.Asks.Count == maxCount)
                {
                    break;
                }
            }
            for (int i = 0; i < token[bids].Count() - 1; i = i + 2)
            {
                var depth = new ExchangeOrderPrice { Price = token[bids][i].ConvertInvariant<decimal>(), Amount = token[bids][i + 1].ConvertInvariant<decimal>() };
                book.Bids[depth.Price] = depth;
                if (book.Asks.Count == maxCount)
                {
                    break;
                }
            }
            return book;
        }

        //下单
        protected override async Task<ExchangeOrderResult> OnPlaceOrderAsync(ExchangeOrderRequest order)
        {
            /*
            {
                "status": 0,
                "data": "9d17a03b852e48c0b3920c7412867623"
            }*/
            Dictionary<string, object> payload = new Dictionary<string, object>();
            RequestMethod = "POST";
            payload.Add("amount", order.Amount.ToString());
            payload.Add("method", "POST");
            payload.Add("price", SelfMath.ToFixed(order.Price, 4).ToString());
            payload.Add("side", order.IsBuy ? "buy" : "sell");
            payload.Add("symbol", NormalizeSymbol(order.Symbol));
            payload.Add("type", order.OrderType.ToString().ToLower());
            JToken obj = await MakeJsonRequestAsync<JToken>($"orders", BaseUrl, payload, "POST");
            ExchangeOrderResult result = new ExchangeOrderResult()
            {
                Amount = order.Amount,
                Price = order.Price,
                IsBuy = order.IsBuy,
                OrderId = obj.ToStringInvariant(),
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
            payload.Add("states", "submitted");
            //payload.Add("before", "submitted");
            //payload.Add("after", "0");
            //payload.Add("limit", "10");

            JToken data = await MakeJsonRequestAsync<JToken>("/orders", BaseUrl, payload, "GET");
            foreach (var prop in data)
            {
                orders.Add(ParseOrder(prop));
            }

            return orders;
        }

        private ExchangeOrderResult ParseOrder(JToken token)
        {
            /*
            {
                "status": 0,
                "data": [
                {
                    "id": "string",
                    "symbol": "string",
                    "type": "limit",
                    "side": "buy",
                    "price": "string",
                    "amount": "string",
                    "state": "submitted",
                    "executed_value": "string",
                    "fill_fees": "string",
                    "filled_amount": "string",
                    "created_at": 0,
                    "source": "web"
                }
                ]
            }*/
            ExchangeOrderResult result = new ExchangeOrderResult()
            {
                OrderId = token["id"].ToStringInvariant(),
                Symbol = token["symbol"].ToStringInvariant(),
                Amount = token["amount"].ConvertInvariant<decimal>(),
                AmountFilled = token["field-amount"].ConvertInvariant<decimal>(),
                Price = token["price"].ConvertInvariant<decimal>(),
                OrderDate = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(token["created-at"].ConvertInvariant<long>()).ToLocalTime(),
                IsBuy = token["type"].ToStringInvariant().StartsWith("buy"),
                Result = ParseState(token["state"].ToStringInvariant()),
            };
            return result;
        }

        private ExchangeAPIOrderResult ParseState(string state)
        {
            switch (state)
            {
                case "submitted":
                    return ExchangeAPIOrderResult.Pending;
                case "partial-filled":
                    return ExchangeAPIOrderResult.FilledPartially;
                case "filled":
                    return ExchangeAPIOrderResult.Filled;
                case "partial-canceled":
                case "canceled":
                case "pending_cancel":
                    return ExchangeAPIOrderResult.Canceled;
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