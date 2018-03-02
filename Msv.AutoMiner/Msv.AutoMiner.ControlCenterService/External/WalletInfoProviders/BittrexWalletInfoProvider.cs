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
    public class BittrexWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        public BittrexWalletInfoProvider(IWebClient webClient, string apiKey, string apiSecret)
            : base(webClient, apiKey, Encoding.UTF8.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
            => DoRequest<JArray>("account", "getbalances")
                .Cast<dynamic>()
                .Select(x => new WalletBalanceData
                {
                    Available = (double) x.Available,
                    Blocked = (double) x.Balance - (double) x.Available,
                    Unconfirmed = (double) x.Pending,
                    CurrencySymbol = (string) x.Currency,
                    Address = (string) x.CryptoAddress
                })
                .ToArray();

        public override WalletOperationData[] GetOperations(DateTime startDate)
            => DoRequest<JArray>("account", "getdeposithistory")
                .Cast<dynamic>()
                .Where(x => (int) x.Confirmations > 0)
                .Select(x => new WalletOperationData
                {
                    ExternalId = (string)x.PaymentUuid,
                    Amount = (double) x.Amount,
                    CurrencySymbol = (string) x.Currency,
                    DateTime = DateTimeHelper.Normalize((DateTime) x.LastUpdated),
                    Address = (string) x.Address,
                    Transaction = (string) x.TxId
                })
                .Concat(DoRequest<JArray>("account", "getwithdrawalhistory")
                    .Cast<dynamic>()
                    .Where(x => (bool) x.Authorized && !(bool) x.PendingPayment && !(bool) x.Canceled)
                    .Select(x => new WalletOperationData
                    {
                        ExternalId = (string)x.PaymentUuid,
                        Amount = -(double) x.Amount,
                        CurrencySymbol = (string) x.Currency,
                        DateTime = DateTimeHelper.Normalize((DateTime) x.Opened),
                        Address = (string) x.Address,
                        Transaction = (string) x.TxId
                    }))
                .ToArray();

        private T DoRequest<T>(string commandType, string command)
            where T : JToken
        {
            var parameters = new Dictionary<string, string>
            {
                ["apikey"] = ApiKey,
                ["nonce"] = DateTime.Now.Ticks.ToString()
            };
            var url = $"https://bittrex.com/api/v1.1/{commandType}/{command}" + new QueryBuilder(parameters);
            using (var hmac = new HMACSHA512(ApiSecret))
            {
                var response = WebClient.DownloadString(
                    url,
                    new Dictionary<string, string>
                    {
                        ["apisign"] = HexHelper.ToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(url)))
                    });
                var json = JsonConvert.DeserializeObject<JObject>(response);
                if (!json["success"].Value<bool>())
                    throw new ExternalDataUnavailableException(json["message"].Value<string>());
                return (T) json["result"];
            }
        }
    }
}
