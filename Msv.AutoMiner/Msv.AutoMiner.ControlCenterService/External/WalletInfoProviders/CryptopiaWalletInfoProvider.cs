using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public class CryptopiaWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        public CryptopiaWalletInfoProvider(IWebClient webClient, string apiKey, string apiSecret)
            : base(webClient, apiKey, Convert.FromBase64String(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances() 
            => DoPostRequest<JArray>("GetBalance")
            .Cast<dynamic>()
            .Select(x => new WalletBalanceData
            {
                CurrencySymbol = (string) x.Symbol,
                Available = (double) x.Available,
                Blocked = (double) x.HeldForTrades + (double) x.PendingWithdraw,
                Unconfirmed = (double) x.Unconfirmed,
                Address = (string) x.Address
            })
            .ToArray();

        public override WalletOperationData[] GetOperations(DateTime startDate)
            => DoPostRequest<JArray>("GetTransactions")
            .Cast<dynamic>()
            .Where(x => (string) x.Status == "Confirmed")
            .Select(x => new WalletOperationData
            {
                ExternalId = (string) x.Id,
                DateTime = DateTimeHelper.Normalize((DateTime) x.Timestamp),
                Amount = (string)x.Type == "Withdraw" ? -(double) x.Amount : (double)x.Amount,
                CurrencySymbol = (string) x.Currency,
                Transaction = (string) x.TxId,
                Address = (string) x.Address
            })
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
                var response = WebClient.UploadString(
                    url,
                    requestJson,
                    new Dictionary<string, string>
                    {
                        ["Authorization"] = $"amx {ApiKey}:{signature}:{nonce}",
                        ["Content-Type"] = "application/json"
                    });
                var json = JsonConvert.DeserializeObject<JObject>(response);
                if (!json["Success"].Value<bool>())
                    throw new ExternalDataUnavailableException(json["Error"].Value<string>());
                return (T) json["Data"];
            }
        }
    }
}
