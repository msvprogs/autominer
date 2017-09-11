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
    public class BittrexExchangeTrader : ExchangeTraderBase
    {
        public BittrexExchangeTrader(string apiKey, string apiSecret)
            : base(apiKey, Encoding.UTF8.GetBytes(apiSecret))
        { }

        public override ExchangeAccountBalanceData[] GetBalances()
            => DoRequest<JArray>("account", "getbalances")
            .Cast<dynamic>()
            .Select(x => new ExchangeAccountBalanceData
            {
                Available = (double) x.Available,
                OnOrders = (double) x.Balance - (double) x.Available,
                CurrencySymbol = (string) x.Currency
            })
            .ToArray();

        public override ExchangeAccountOperationData[] GetOperations(DateTime startDate)
        {
            var deposits = DoRequest<JArray>("account", "getdeposithistory");
            var withdrawals = DoRequest<JArray>("account", "getwithdrawalhistory");

            return deposits
                .Cast<dynamic>()
                .Where(x => (int)x.Confirmations > 0)
                .Select(x => new ExchangeAccountOperationData
                {
                    Amount = (double)x.Amount,
                    CurrencySymbol = (string)x.Currency,
                    DateTime = TimestampHelper.ToLocalNormalized((DateTime)x.LastUpdated)
                })
                .Concat(withdrawals
                    .Cast<dynamic>()
                    .Where(x => (bool)x.Authorized && !(bool)x.PendingPayment && !(bool)x.Canceled)
                    .Select(x => new ExchangeAccountOperationData
                    {
                        Amount = -(double)x.Amount,
                        CurrencySymbol = (string)x.Currency,
                        DateTime = TimestampHelper.ToLocalNormalized((DateTime)x.Opened)
                    }))
                .Where(x => x.DateTime > startDate)
                .ToArray();
        }

        private T DoRequest<T>(string commandType, string command)
            where T : JToken
        {
            var parameters = new Dictionary<string, string>
            {
                ["apikey"] = ApiKey,
                ["nonce"] = DateTime.Now.Ticks.ToString()
            };
            var url = new UriBuilder($"https://bittrex.com/api/v1.1/{commandType}/{command}")
            {
                Query = string.Join("&", parameters.Select(x => $"{x.Key}={x.Value}"))
            }.Uri;
            using (var hmac = new HMACSHA512(ApiSecret))
            {
                var response = DownloadString(
                    url.ToString(),
                    new Dictionary<string, string>
                    {
                        ["apisign"] = HexHelper.ToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(url.ToString())))
                    });
                var json = JsonConvert.DeserializeObject<JObject>(response);
                if (!json["success"].Value<bool>())
                    throw new ApplicationException(json["message"].Value<string>());
                return (T)json["result"];
            }
        }
    }
}
