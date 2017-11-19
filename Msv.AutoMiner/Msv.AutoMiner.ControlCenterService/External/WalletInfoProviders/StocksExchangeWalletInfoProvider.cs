using System;
using System.Collections.Generic;
using System.Dynamic;
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
    public class StocksExchangeWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        public StocksExchangeWalletInfoProvider(IWebClient webClient, string apiKey, string apiSecret)
            : base(webClient, apiKey, Encoding.UTF8.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
        {
            var info = DoPostRequest("GetInfo");
            return ((JObject)ToJObject(info.funds)).Properties()
                .Join(((JObject)ToJObject(info.hold_funds)).Properties(),
                    x => x.Name, x => x.Name, (x, y) => (available:x, hold:y))
                .Join(((JObject)ToJObject(info.wallets_addresses)).Properties(),
                    x => x.available.Name, x => x.Name, (x, y) => (x.available, x.hold, address: y))
                .Select(x => new WalletBalanceData
                {
                    CurrencySymbol = x.available.Name,
                    Address = x.address.Value.Value<string>(),
                    Available = x.available.Value.Value<double>(),
                    Blocked = x.hold.Value.Value<double>()
                })
                .ToArray();
        }

        public override WalletOperationData[] GetOperations(DateTime startDate)
        {
            dynamic parameters = new ExpandoObject();
            parameters.currency = "ALL";
            var operations = DoPostRequest("TransHistory", parameters);
            return ((JObject)ToJObject(operations.DEPOSIT))
                .Properties()
                .Select(x => new {x.Name, Value = (dynamic) x.Value})
                .Where(x => "finished".Equals((string) x.Value.Status, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => new WalletOperationData
                {
                    ExternalId = x.Name,
                    Amount = (double) x.Value.Amount,
                    Transaction = (string) x.Value.TX_id,
                    CurrencySymbol = (string) x.Value.Currency,
                    DateTime = DateTimeHelper.ToDateTimeUtc((long) x.Value.Date)
                })
                .Concat(((JObject)ToJObject(operations.WITHDRAWAL))
                    .Properties()
                    .Select(x => new {x.Name, Value = (dynamic) x.Value})
                    .Where(x => "finished".Equals((string) x.Value.Status, StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => new WalletOperationData
                    {
                        ExternalId = x.Name,
                        Amount = -(double) x.Value.Amount,
                        Transaction = (string) x.Value.TX_id,
                        CurrencySymbol = (string) x.Value.Currency,
                        DateTime = DateTimeHelper.ToDateTimeUtc((long) x.Value.Date),
                        Address = (string) x.Value.Address
                    }))
                .Where(x => x.DateTime > startDate)
                .ToArray();
        }

        private static JObject ToJObject(dynamic objectOrArray)
        {
            // If JObject is empty, it becomes an empty array...
            if (objectOrArray is JArray)
                return new JObject();
            return (JObject) objectOrArray;
        }

        private dynamic DoPostRequest(string method, dynamic parameters = null)
        {
            if (parameters == null)
                parameters = new ExpandoObject();
            parameters.method = method;
            parameters.nonce = DateTime.Now.Ticks;
            string serializedData = JsonConvert.SerializeObject(parameters);
            using (var hmac = new HMACSHA512(ApiSecret))
            {
                var response = WebClient.UploadString(
                    "https://stocks.exchange/api2",
                    serializedData,
                    new Dictionary<string, string>
                    {
                        ["Content-Type"] = "application/json",
                        ["Key"] = ApiKey,
                        ["Sign"] = HexHelper.ToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(serializedData)))
                    });
                var json = JsonConvert.DeserializeObject<JObject>(response);
                if (json["success"].Value<int>() == 0)
                    throw new ExternalDataUnavailableException(json["error"]?.Value<string>());
                return json["data"];
            }
        }
    }
}
