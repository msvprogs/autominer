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
    public class TradeSatoshiWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        private const int MaxOperationCount = 30;

        public TradeSatoshiWalletInfoProvider(IWebClient webClient, string apiKey, string apiSecret)
            : base(webClient, apiKey, Convert.FromBase64String(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
            => DoPostRequest<JArray>("GetBalances", new JObject())
                .Cast<dynamic>()
                .Select(x => new WalletBalanceData
                {
                    CurrencySymbol = (string) x.currency,
                    Available = (double) x.available,
                    Blocked = (double) x.heldForTrades + (double) x.pendingWithdraw,
                    Unconfirmed = (double) x.unconfirmed,
                    Address = (string) x.address
                })
                .ToArray();

        public override WalletOperationData[] GetOperations(DateTime startDate)
            => DoPostRequest<JArray>("GetDeposits", new {Count = MaxOperationCount})
                .Cast<dynamic>()
                .Where(x => (string) x.status == "Confirmed")
                .Select(x => new WalletOperationData
                {
                    ExternalId = (string) x.id,
                    CurrencySymbol = (string) x.currency,
                    Amount = (double) x.amount,
                    DateTime = DateTimeHelper.Normalize((DateTime) x.timeStamp),
                    Transaction = (string) x.txid
                })
                .Concat(DoPostRequest<JArray>("GetWithdrawals", new {Count = MaxOperationCount})
                    .Cast<dynamic>()
                    .Where(x => (string) x.status == "Complete")
                    .Select(x => new WalletOperationData
                    {
                        ExternalId = (string) x.id,
                        CurrencySymbol = (string) x.currency,
                        Amount = -(double) x.amount,
                        DateTime = DateTimeHelper.Normalize((DateTime) x.timeStamp),
                        Transaction = (string) x.txid,
                        Address = (string) x.address
                    }))
                .ToArray();

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
                var response = WebClient.UploadString(
                    url,
                    requestJson,
                    new Dictionary<string, string>
                    {
                        ["Authorization"] = $"amx {ApiKey}:{signature}:{nonce}",
                        ["Content-Type"] = "application/json"
                    });
                var json = JsonConvert.DeserializeObject<JObject>(response);
                if (!json["success"].Value<bool>())
                    throw new ExternalDataUnavailableException(json["message"].Value<string>());
                return (T) json["result"];
            }
        }
    }
}
