using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.Exchanges.Api;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public class StocksExchangeWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        public StocksExchangeWalletInfoProvider(IExchangeApi api, string apiKey, string apiSecret)
            : base(api, apiKey, Encoding.UTF8.GetBytes(apiSecret))
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
            var operations = DoPostRequest("TransHistory", new Dictionary<string, string>
            {
                ["currency"] = "ALL"
            });
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

        private dynamic DoPostRequest(string method, Dictionary<string, string> parameters = null)
            => Api.ExecutePrivate(method, parameters ?? new Dictionary<string, string>(), ApiKey, ApiSecret);
    }
}
