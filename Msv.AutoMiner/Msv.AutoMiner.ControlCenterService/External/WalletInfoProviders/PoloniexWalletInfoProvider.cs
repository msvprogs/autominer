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
    public class PoloniexWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        public PoloniexWalletInfoProvider(IExchangeApi api, string apiKey, string apiSecret)
            : base(api, apiKey, Encoding.UTF8.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
            => ((JObject)Api.ExecutePrivate(
                    "returnCompleteBalances", new Dictionary<string, string>(), ApiKey, ApiSecret))
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
            var operations = Api.ExecutePrivate("returnDepositsWithdrawals",
                new Dictionary<string, string>
                {
                    ["start"] = DateTimeHelper.ToTimestamp(startDate).ToString(),
                    ["end"] = DateTimeHelper.NowTimestamp.ToString()
                },
                ApiKey, 
                ApiSecret);
            return ((JArray)operations.deposits)
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
                .Concat(((JArray)operations.withdrawals)
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
    }
}
