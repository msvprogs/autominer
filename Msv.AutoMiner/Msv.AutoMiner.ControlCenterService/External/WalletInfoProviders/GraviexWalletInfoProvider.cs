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
    public class GraviexWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        public GraviexWalletInfoProvider(IExchangeApi api, string apiKey, string apiSecret) 
            : base(api, apiKey, Encoding.ASCII.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
            => ((JArray) DoGetRequest("members/me.json").accounts)
                .Cast<dynamic>()
                .Select(x => new WalletBalanceData
                {
                    CurrencySymbol = ((string) x.currency).ToUpperInvariant(),
                    Available = (double) x.balance,
                    Blocked = (double) x.locked
                })
                .ToArray();

        //TODO: withdrawals
        public override WalletOperationData[] GetOperations(DateTime startDate) 
            => ((JArray) DoGetRequest("deposits.json"))
                .Cast<dynamic>()
                .Where(x => x.state == "accepted")
                .Select(x => new WalletOperationData
                {
                    DateTime = DateTimeHelper.Normalize((DateTime)x.created_at),
                    ExternalId = (string) x.id,
                    CurrencySymbol = ((string) x.currency).ToUpperInvariant(),
                    Amount = (double) x.amount,
                    Transaction = (string) x.txid
                })
                .Where(x => x.DateTime > startDate)
                .ToArray();

        private dynamic DoGetRequest(string command, IDictionary<string, string> parameters = null) 
            => Api.ExecutePrivate(command, parameters ?? new Dictionary<string, string>(), ApiKey, ApiSecret);
    }
}
