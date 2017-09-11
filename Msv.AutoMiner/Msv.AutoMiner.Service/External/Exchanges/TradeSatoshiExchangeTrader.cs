using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Exchanges
{
    public class TradeSatoshiExchangeTrader : ExchangeTraderBase
    {
        public TradeSatoshiExchangeTrader(string apiKey, string apiSecret)
            : base(apiKey, Convert.FromBase64String(apiSecret))
        { }

        public override ExchangeAccountBalanceData[] GetBalances() 
            => DoPostRequest<JArray>("GetBalances", new JObject())
            .Cast<dynamic>()
            .Select(x => new ExchangeAccountBalanceData
            {
                CurrencySymbol = (string) x.currency,
                Available = (double) x.available,
                OnOrders = (double) x.heldForTrades
            })
            .ToArray();

        public override ExchangeAccountOperationData[] GetOperations(DateTime startDate)
        {
            var pars = new { Count = 30 };
            var deposits = DoPostRequest<JArray>("GetDeposits", pars);
            var withdrawals = DoPostRequest<JArray>("GetWithdrawals", pars);
            return deposits.Cast<dynamic>()
                .Where(x => (string) x.status == "Confirmed")
                .Select(x => new ExchangeAccountOperationData
                {
                    CurrencySymbol = (string) x.currency,
                    Amount = (double) x.amount,
                    DateTime = TimestampHelper.ToLocalNormalized((DateTime) x.timeStamp)
                })
                .Concat(withdrawals.Cast<dynamic>()
                    .Where(x => (string) x.status == "Complete")
                    .Select(x => new ExchangeAccountOperationData
                    {
                        CurrencySymbol = (string) x.currency,
                        Amount = -(double) x.amount,
                        DateTime = TimestampHelper.ToLocalNormalized((DateTime) x.timeStamp)
                    }))
                .Where(x => x.DateTime > startDate)
                .ToArray();
        }

        private T DoPostRequest<T>(string command, object request)
            where T : JToken
        {
            using (var hmac = new HMACSHA512(ApiSecret))
            {
                var requestJson = JsonConvert.SerializeObject(request);
                var url = $"https://tradesatoshi.com/api/private/{command}";
                var nonce = DateTime.Now.Ticks.ToString();
                var signature = Convert.ToBase64String(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(
                        string.Concat(ApiKey,
                            "POST",
                            Uri.EscapeDataString(url).ToLowerInvariant(),
                            nonce,
                            Convert.ToBase64String(Encoding.UTF8.GetBytes(requestJson))))));
                var response = UploadString(
                    url,
                    requestJson,
                    new Dictionary<string, string>
                    {
                        ["Authorization"] = $"amx {ApiKey}:{signature}:{nonce}",
                        ["Content-Type"] = "application/json"
                    });
                var json = JsonConvert.DeserializeObject<JObject>(response);
                if (!json["success"].Value<bool>())
                    throw new ApplicationException(json["message"].Value<string>());
                return (T) json["result"];
            }
        }
    }
}
