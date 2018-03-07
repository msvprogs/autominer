using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.Exchanges.Api;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public class TradeSatoshiWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        // TradeSatoshi returns operations very selectively with low limit. It can return every 10-th, 5-th operation and so on.
        private const int MaxOperationCount = 1000;

        public TradeSatoshiWalletInfoProvider(IExchangeApi api, string apiKey, string apiSecret)
            : base(api, apiKey, Convert.FromBase64String(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
            => DoPostRequest<JArray>("GetBalances")
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
            => DoPostRequest<JArray>("GetDeposits", new Dictionary<string, string>
                {
                    ["Count"] = MaxOperationCount.ToString()
                })
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
                .Concat(DoPostRequest<JArray>("GetWithdrawals", new Dictionary<string, string>
                    {
                        ["Count"] = MaxOperationCount.ToString()
                    })
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

        private T DoPostRequest<T>(string command, Dictionary<string, string> parameters = null)
            where T : JToken
            => (T) Api.ExecutePrivate(command, parameters ?? new Dictionary<string, string>(), ApiKey, ApiSecret);
    }
}
