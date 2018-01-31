using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public class NovaexchangeWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        public NovaexchangeWalletInfoProvider(IWebClient webClient, string apiKey, string apiSecret)
            : base(webClient, apiKey, Encoding.ASCII.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
            => DoPostRequest<JArray>("getbalances", "balances")
                .Cast<dynamic>()
                .Select(x => new WalletBalanceData
                {
                    CurrencySymbol = (string) x.currency,
                    Available = (double) x.amount,
                    Blocked = (double) x.amount_trades + (double)x.amount_lockbox
                })
                .ToArray();

        public override WalletOperationData[] GetOperations(DateTime startDate)
            => DoPostRequest<JArray>("getdeposithistory", "items")
                .Cast<dynamic>()
                .Where(x => (string) x.status == "Accounted")
                .Select(x => new WalletOperationData
                {
                    CurrencySymbol = (string) x.currency,
                    Amount = (double) x.tx_amount,
                    DateTime = DateTime.ParseExact(
                        (string) x.time_seen, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    Address = (string) x.tx_address,
                    Transaction = (string) x.tx_txid
                })
                .Concat(DoPostRequest<JArray>("getwithdrawalhistory", "items")
                    .Cast<dynamic>()
                    .Where(x => (string)x.status == "Confirmed")
                    .Select(x => new WalletOperationData
                    {
                        CurrencySymbol = (string)x.currency,
                        Amount = -(double)x.tx_amount,
                        DateTime = DateTime.ParseExact(
                            (string)x.time_sent, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        Address = (string) x.tx_address,
                        Transaction = (string) x.tx_txid
                    }))
                .ToArray();

        private T DoPostRequest<T>(string method, string resultKey)
            where T : JToken
        {
            using (var hmac = new HMACSHA512(ApiSecret))
            {
                var url = $"https://novaexchange.com/remote/v2/private/{method}/?nonce={DateTime.Now.Ticks}";
                var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(url)));
                var response = WebClient.UploadString(
                    url,
                    $"apikey={Uri.EscapeDataString(ApiKey)}&signature={Uri.EscapeDataString(signature)}",
                    null,
                    contentType: "application/x-www-form-urlencoded");
                var json = JsonConvert.DeserializeObject<JObject>(response);
                if (json["status"].Value<string>() != "success")
                    throw new ExternalDataUnavailableException(json["message"].Value<string>());
                return (T) json[resultKey];
            }
        }
    }
}
