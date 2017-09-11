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
    public class PoloniexExchangeTrader : ExchangeTraderBase
    {
        public PoloniexExchangeTrader(string apiKey, string apiSecret)
            : base(apiKey, Encoding.UTF8.GetBytes(apiSecret))
        { }

        public override ExchangeAccountBalanceData[] GetBalances()
            => DoPostRequest("returnCompleteBalances", new Dictionary<string, string>())
                .Properties()
                .Select(x => new
                {
                    x.Name,
                    Data = (dynamic) x.Value
                })
                .Select(x => new ExchangeAccountBalanceData
                {
                    CurrencySymbol = x.Name,
                    Available = (double) x.Data.available,
                    OnOrders = (double) x.Data.onOrders
                })
                .ToArray();

        public override ExchangeAccountOperationData[] GetOperations(DateTime startDate)
        {
            var operations = DoPostRequest("returnDepositsWithdrawals",
                new Dictionary<string, string>
                {
                    ["start"] = TimestampHelper.ToTimestamp(startDate, TimeZoneInfo.Local).ToString(),
                    ["end"] = TimestampHelper.Now.ToString()
                });
            return operations["deposits"]
                .Cast<dynamic>()
                .Where(x => (string) x.status == "COMPLETE")
                .Select(x => new ExchangeAccountOperationData
                {
                    Amount = (double) x.amount,
                    CurrencySymbol = (string) x.currency,
                    DateTime = TimestampHelper.ToDateTime((long) x.timestamp)
                })
                .Concat(operations["withdrawals"]
                    .Cast<dynamic>()
                    .Where(x => ((string) x.status).StartsWith("COMPLETE"))
                    .Select(x => new ExchangeAccountOperationData
                    {
                        Amount = -(double) x.amount,
                        CurrencySymbol = (string) x.currency,
                        DateTime = TimestampHelper.ToDateTime((long) x.timestamp)
                    }))
                .Where(x => x.DateTime > startDate)
                .ToArray();
        }

        private JObject DoPostRequest(string command, Dictionary<string, string> parameters)
        {
            parameters.Add("command", command);
            parameters.Add("nonce", DateTime.Now.Ticks.ToString());
            var queryString = string.Join("&", parameters.Select(x => $"{x.Key}={x.Value}"));
            using (var hmac = new HMACSHA512(ApiSecret))
            {
                var response = UploadString(
                    "https://poloniex.com/tradingApi",
                    queryString,
                    new Dictionary<string, string>
                    {
                        ["Content-Type"] = "application/x-www-form-urlencoded",
                        ["Key"] = ApiKey,
                        ["Sign"] = HexHelper.ToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString)))
                    });
                var json = JsonConvert.DeserializeObject<JObject>(response);
                if (json["error"] != null)
                    throw new ApplicationException(json["error"].Value<string>());
                return json;
            }
        }
    }
}
