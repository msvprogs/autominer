using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public class PoloniexWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        public PoloniexWalletInfoProvider(IWebClient webClient, string apiKey, string apiSecret)
            : base(webClient, apiKey, Encoding.UTF8.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
            => DoPostRequest("returnCompleteBalances", new Dictionary<string, string>())
                .Properties()
                .Select(x => new
                {
                    x.Name,
                    Data = (dynamic) x.Value
                })
                .Select(x => new WalletBalanceData
                {
                    CurrencySymbol = x.Name,
                    Available = (double) x.Data.available,
                    Blocked = (double) x.Data.onOrders
                })
                .ToArray();

        public override WalletOperationData[] GetOperations(DateTime startDate)
        {
            var operations = DoPostRequest("returnDepositsWithdrawals",
                new Dictionary<string, string>
                {
                    ["start"] = DateTimeHelper.ToTimestamp(startDate).ToString(),
                    ["end"] = DateTimeHelper.NowTimestamp.ToString()
                });
            return operations["deposits"]
                .Cast<dynamic>()
                .Where(x => (string) x.status == "COMPLETE")
                .Select(x => new WalletOperationData
                {
                    Amount = (double) x.amount,
                    CurrencySymbol = (string) x.currency,
                    DateTime = DateTimeHelper.ToDateTimeUtc((long) x.timestamp),
                    Address = (string) x.address,
                    Transaction = (string) x.txid
                })
                .Concat(operations["withdrawals"]
                    .Cast<dynamic>()
                    .Where(x => ((string) x.status).StartsWith("COMPLETE:"))
                    .Select(x => new WalletOperationData
                    {
                        Amount = -(double) x.amount,
                        CurrencySymbol = (string) x.currency,
                        DateTime = DateTimeHelper.ToDateTimeUtc((long) x.timestamp),
                        Address = (string) x.address,
                        Transaction = ((string) x.status).Split(':').Last().Trim(),
                        ExternalId = (string) x.withdrawalNumber
                    }))
                .ToArray();
        }

        private JObject DoPostRequest(string command, Dictionary<string, string> parameters)
        {
            parameters.Add("command", command);
            parameters.Add("nonce", DateTime.Now.Ticks.ToString());
            var queryString = new QueryBuilder(parameters).ToStringWithoutPrefix();
            using (var hmac = new HMACSHA512(ApiSecret))
            {
                var response = WebClient.UploadString(
                    "https://poloniex.com/tradingApi",
                    queryString,
                    new Dictionary<string, string>
                    {
                        ["Key"] = ApiKey,
                        ["Sign"] = HexHelper.ToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString)))
                    },
                    contentType: "application/x-www-form-urlencoded");
                var json = JsonConvert.DeserializeObject<JObject>(response);
                if (json["error"] != null)
                    throw new ExternalDataUnavailableException(json["error"].Value<string>());
                return json;
            }
        }
    }
}
