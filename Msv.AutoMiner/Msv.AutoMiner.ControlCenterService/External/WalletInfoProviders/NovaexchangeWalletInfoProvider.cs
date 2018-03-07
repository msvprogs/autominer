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
    public class NovaexchangeWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        public NovaexchangeWalletInfoProvider(IExchangeApi api, string apiKey, string apiSecret)
            : base(api, apiKey, Encoding.ASCII.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
            => ((JArray)DoPostRequest("getbalances").items)
                .Cast<dynamic>()
                .Select(x => new WalletBalanceData
                {
                    CurrencySymbol = (string) x.currency,
                    Available = (double) x.amount,
                    Blocked = (double) x.amount_trades + (double)x.amount_lockbox
                })
                .ToArray();

        public override WalletOperationData[] GetOperations(DateTime startDate)
            => ((JArray)DoPostRequest("getdeposithistory").items)
                .Cast<dynamic>()
                .Where(x => (string) x.status == "Accounted")
                .Select(x => new WalletOperationData
                {
                    CurrencySymbol = (string) x.currency,
                    Amount = (double) x.tx_amount,
                    DateTime = DateTimeHelper.FromIso8601((string) x.time_seen),
                    Address = (string) x.tx_address,
                    Transaction = (string) x.tx_txid
                })
                .Concat(((JArray)DoPostRequest("getwithdrawalhistory").items)
                    .Cast<dynamic>()
                    .Where(x => (string)x.status == "Confirmed")
                    .Select(x => new WalletOperationData
                    {
                        CurrencySymbol = (string)x.currency,
                        Amount = -(double)x.tx_amount,
                        DateTime = DateTimeHelper.FromIso8601((string)x.time_sent),
                        Address = (string) x.tx_address,
                        Transaction = (string) x.tx_txid
                    }))
                .ToArray();

        private dynamic DoPostRequest(string method)
            => Api.ExecutePrivate(method, new Dictionary<string, string>(), ApiKey, ApiSecret);
    }
}
