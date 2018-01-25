using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    //API: https://btc-alpha.github.io/api-docs
    public class BtcAlphaWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://btc-alpha.com/api/v1/");

        public BtcAlphaWalletInfoProvider(IWebClient webClient, string apiKey, string apiSecret)
            : base(webClient, apiKey, Encoding.ASCII.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances() 
            => ((JArray) DoGetRequest("wallets"))
                .Cast<dynamic>()
                .Select(x => new WalletBalanceData
                {
                    CurrencySymbol = (string) x.currency,
                    Available = (double) x.balance,
                    Blocked = (double) x.reserve
                })
                .ToArray();

        public override WalletOperationData[] GetOperations(DateTime startDate)
        {
            // Wait a second! I have a limit of 2 requests/sec per account!
            Thread.Sleep(1000);

            return ((JArray) DoGetRequest("deposits"))
                .Cast<dynamic>()
                .Select(x => new WalletOperationData
                {
                    ExternalId = (string) x.id,
                    CurrencySymbol = (string) x.currency,
                    Amount = (double) x.amount,
                    DateTime = DateTimeHelper.ToDateTimeUtc((long) (double) x.timestamp)
                })
                .Concat(((JArray) DoGetRequest("withdraws"))
                    .Cast<dynamic>()
                    .Where(x => (int) x.status == 30 /* Approved */)
                    .Select(x => new WalletOperationData
                    {
                        ExternalId = (string) x.id,
                        CurrencySymbol = (string) x.currency,
                        Amount = -(double) x.amount,
                        DateTime = DateTimeHelper.ToDateTimeUtc((long) (double) x.timestamp)
                    }))
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
                var response = WebClient.DownloadString(
                    new Uri(M_BaseUri, relativeUrl + (queryString != "" ? "?" + queryString : "")).ToString(),
                    new Dictionary<string, string>
                    {
                        ["X-KEY"] = ApiKey,
                        ["X-SIGN"] = HexHelper.ToHex(
                            hmac.ComputeHash(Encoding.UTF8.GetBytes(ApiKey + queryString))),
                        ["X-NONCE"] = DateTime.UtcNow.Ticks.ToString()
                    });
                return JsonConvert.DeserializeObject<dynamic>(response);
            }
        }
    }
}
