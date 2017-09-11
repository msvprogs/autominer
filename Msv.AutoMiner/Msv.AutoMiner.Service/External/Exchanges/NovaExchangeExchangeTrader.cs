using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Msv.AutoMiner.Service.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Exchanges
{
    public class NovaexchangeExchangeTrader : ExchangeTraderBase
    {
        public NovaexchangeExchangeTrader(string apiKey, string apiSecret)
            : base(apiKey, Encoding.ASCII.GetBytes(apiSecret))
        { }

        public override ExchangeAccountBalanceData[] GetBalances()
            => DoPostRequest<JArray>("getbalances", "balances")
                .Cast<dynamic>()
                .Select(x => new ExchangeAccountBalanceData
                {
                    CurrencySymbol = (string) x.currency,
                    Available = (double) x.amount,
                    OnOrders = (double) x.amount_trades
                })
                .ToArray();

        public override ExchangeAccountOperationData[] GetOperations(DateTime startDate)
            => DoPostRequest<JArray>("getdeposithistory", "items")
                .Cast<dynamic>()
                .Where(x => (string) x.status == "Accounted")
                .Select(x => new ExchangeAccountOperationData
                {
                    CurrencySymbol = (string) x.currency,
                    Amount = (double) x.tx_amount,
                    DateTime = DateTime.ParseExact(
                        (string) x.time_seen, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                })
                .Concat(DoPostRequest<JArray>("getwithdrawalhistory", "items")
                    .Cast<dynamic>()
                    .Where(x => (string)x.status == "Confirmed")
                    .Select(x => new ExchangeAccountOperationData
                    {
                        CurrencySymbol = (string)x.currency,
                        Amount = -(double)x.tx_amount,
                        DateTime = DateTime.ParseExact(
                            (string)x.time_sent, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                    }))
                .Where(x => x.DateTime > startDate)
                .ToArray();

        private T DoPostRequest<T>(string method, string resultKey)
            where T : JToken
        {
            using (var hmac = new HMACSHA512(ApiSecret))
            {
                var nonce = DateTime.Now.Ticks.ToString();
                var url = $"https://novaexchange.com/remote/v2/private/{method}/?nonce={nonce}";
                var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(url)));
                var response = UploadString(
                    url,
                    $"apikey={Uri.EscapeDataString(ApiKey)}&signature={Uri.EscapeDataString(signature)}",
                    new Dictionary<string, string>
                    {
                        ["Content-Type"] = "application/x-www-form-urlencoded"
                    });
                var json = JsonConvert.DeserializeObject<JObject>(response);
                if (json["status"].Value<string>() != "success")
                    throw new ApplicationException(json["message"].Value<string>());
                return (T) json[resultKey];
            }
        }
    }
}
