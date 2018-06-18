﻿using System;
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
            NonceStyle = NonceStyle.UnixSecondsString;   // not used, see below
            SymbolSeparator = string.Empty;
            SymbolIsUppercase = false;
            //SymbolIsReversed = true;
        }
        public override string NormalizeSymbol(string symbol)
        {
            return (symbol ?? string.Empty).Replace("-", string.Empty).Replace("/", string.Empty).Replace("_", string.Empty);
        }
        protected override async Task ProcessRequestAsync(HttpWebRequest request, Dictionary<string, object> payload)
        {
            request.Headers = new WebHeaderCollection(){
                {"FC-ACCESS-KEY", PublicApiKey.ToUnsecureString()},
                {"FC-ACCESS-SIGNATURE", sig},
                {"FC-ACCESS-TIMESTAMP",timestamp }
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
        protected override async Task<IEnumerable<ExchangeOrderResult>> OnGetOpenOrderDetailsAsync(string symbol = null)
        {
            if (symbol == null) { throw new APIException("symbol cannot be null"); }

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
