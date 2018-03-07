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
    public class BtcAlphaWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        public BtcAlphaWalletInfoProvider(IExchangeApi api, string apiKey, string apiSecret)
            : base(api, apiKey, Encoding.ASCII.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances() 
            => ((JArray) DoGetRequest("wallets"))
                .Cast<dynamic>()
                .Select(x => new WalletBalanceData
                {
                    CurrencySymbol = (string) x.currency,
                    Available = (double) x.balance,
                    Blocked = (double) x.reserve
                })
                .ToArray();

        public override WalletOperationData[] GetOperations(DateTime startDate)
        {
            return ((JArray) DoGetRequest("deposits"))
                .Cast<dynamic>()
                .Select(x => new WalletOperationData
                {
                    ExternalId = (string) x.id,
                    CurrencySymbol = (string) x.currency,
                    Amount = (double) x.amount,
                    DateTime = DateTimeHelper.ToDateTimeUtc((long) (double) x.timestamp)
                })
                .Concat(((JArray) DoGetRequest("withdraws"))
                    .Cast<dynamic>()
                    .Where(x => (int) x.status == 30 /* Approved */)
                    .Select(x => new WalletOperationData
                    {
                        ExternalId = (string) x.id,
                        CurrencySymbol = (string) x.currency,
                        Amount = -(double) x.amount,
                        DateTime = DateTimeHelper.ToDateTimeUtc((long) (double) x.timestamp)
                    }))
                .Where(x => x.DateTime > startDate)
                .ToArray();
        }

        private dynamic DoGetRequest(string command, Dictionary<string, string> parameters = null) 
            => Api.ExecutePrivate(command, parameters ?? new Dictionary<string, string>(), ApiKey, ApiSecret);
    }
}
