using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public class LiveCoinWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://api.livecoin.net/");

        public LiveCoinWalletInfoProvider(IWebClient webClient, string apiKey, string apiSecret) 
            : base(webClient, apiKey, Encoding.ASCII.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
            => ((JArray) DoGetRequest("/payment/balances"))
                .Cast<dynamic>()
                .Select(x => new
                {
                    Type = (string) x.type,
                    Currency = (string) x.currency,
                    Value = (double) x.value
                })
                .GroupBy(x => x.Currency)
                .Select(x => new
                {
                    Currency = x.Key,
                    Values = x.ToDictionary(y => y.Type, y => y.Value)
                })
                .Select(x => new WalletBalanceData
                {
                    CurrencySymbol = x.Currency,
                    Available = x.Values.TryGetValue("available"),
                    Blocked = x.Values.TryGetValue("trade")
                })
                .ToArray();

        public override WalletOperationData[] GetOperations(DateTime startDate)
        {
            JArray result = DoGetRequest("/payment/history/transactions",
                new Dictionary<string, string>
                {
                    ["start"] = DateTimeHelper.ToTimestampMsec(startDate, TimeZoneInfo.Utc).ToString(),
                    ["end"] = DateTimeHelper.NowTimestampMsec.ToString(),
                    ["types"] = "DEPOSIT,WITHDRAWAL"
                });
            return result
                .Cast<dynamic>()
                .Select(x => new WalletOperationData
                {
                    DateTime = DateTimeHelper.ToDateTimeUtcMsec((long) x.date),
                    Amount = (string) x.type == "WITHDRAWAL"
                        ? -(double) x.amount
                        : (double) x.amount,
                    CurrencySymbol = (string) x.fixedCurrency,
                    Transaction = (string) x.id
                })
                .Where(x => x.DateTime > startDate)
                .ToArray();
        }

        private dynamic DoGetRequest(string relativeUrl, Dictionary<string, string> parameters = null)
        {
            using (var hmac = new HMACSHA256(ApiSecret))
            {
                var queryString = string.Join("&", (parameters ?? new Dictionary<string, string>())
                    .OrderBy(x => x.Key)
                    .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
                if (queryString != string.Empty)
                    relativeUrl += "?" + queryString;
                var response = WebClient.DownloadString(
                    new Uri(M_BaseUri, relativeUrl).ToString(),
                    new Dictionary<string, string>
                    {
                        ["Api-Key"] = ApiKey,
                        ["Sign"] = HexHelper.ToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString))).ToUpperInvariant()
                    });
                return JsonConvert.DeserializeObject<dynamic>(response);
            }
        }
    }
}
