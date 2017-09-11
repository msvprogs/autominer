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
    public class CryptopiaExchangeTrader : ExchangeTraderBase
    {
        public CryptopiaExchangeTrader(string apiKey, string apiSecret)
            : base(apiKey, Convert.FromBase64String(apiSecret))
        { }

        public override ExchangeAccountBalanceData[] GetBalances() 
            => DoPostRequest<JArray>("GetBalance")
            .Cast<dynamic>()
            .Select(x => new ExchangeAccountBalanceData
            {
                CurrencySymbol = (string) x.Symbol,
                Available = (double) x.Available,
                OnOrders = (double) x.HeldForTrades
            })
            .ToArray();

        public override ExchangeAccountOperationData[] GetOperations(DateTime startDate)
            => DoPostRequest<JArray>("GetTransactions")
            .Cast<dynamic>()
            .Where(x => (string) x.Status == "Confirmed")
            .Select(x => new ExchangeAccountOperationData
            {
                DateTime = TimestampHelper.ToLocalNormalized((DateTime) x.Timestamp),
                Amount = (string)x.Type == "Withdraw" ? -(double) x.Amount : (double)x.Amount,
                CurrencySymbol = (string) x.Currency
            })
            .Where(x => x.DateTime > startDate)
            .ToArray();

        private T DoPostRequest<T>(string command)
            where T : JToken
        {
            using (var hmac = new HMACSHA256(ApiSecret))
            using (var md5 = MD5.Create())
            {
                const string requestJson = "{}";
                var url = $"https://www.cryptopia.co.nz/api/{command}";
                var nonce = DateTime.Now.Ticks.ToString();
                var signature = Convert.ToBase64String(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(
                        string.Concat(ApiKey,
                            "POST",
                            Uri.EscapeDataString(url).ToLowerInvariant(),
                            nonce,
                            Convert.ToBase64String(md5.ComputeHash(
                                Encoding.UTF8.GetBytes(requestJson)))))));
                var response = UploadString(
                    url,
                    requestJson,
                    new Dictionary<string, string>
                    {
                        ["Authorization"] = $"amx {ApiKey}:{signature}:{nonce}",
                        ["Content-Type"] = "application/json"
                    });
                var json = JsonConvert.DeserializeObject<JObject>(response);
                if (!json["Success"].Value<bool>())
                    throw new ApplicationException(json["Error"].Value<string>());
                return (T) json["Data"];
            }
        }
    }
}
