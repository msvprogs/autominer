using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.Exchanges.Api;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public class LiveCoinWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        public LiveCoinWalletInfoProvider(IExchangeApi api, string apiKey, string apiSecret) 
            : base(api, apiKey, Encoding.ASCII.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
            => ((JArray) DoGetRequest("payment/balances"))
                .Cast<dynamic>()
                .Select(x => new
                {
                    Type = (string) x.type,
                    Currency = (string) x.currency,
                    Value = (double) x.value
                })
                .GroupBy(x => x.Currency)
                .Select(x => new
                {
                    Currency = x.Key,
                    Values = x.ToDictionary(y => y.Type, y => y.Value)
                })
                .Select(x => new WalletBalanceData
                {
                    CurrencySymbol = x.Currency,
                    Available = x.Values.TryGetValue("available"),
                    Blocked = x.Values.TryGetValue("trade")
                })
                .ToArray();

        public override WalletOperationData[] GetOperations(DateTime startDate)
        {
            JArray result = DoGetRequest("payment/history/transactions",
                new Dictionary<string, string>
                {
                    ["start"] = DateTimeHelper.ToTimestampMsec(startDate).ToString(),
                    ["end"] = DateTimeHelper.NowTimestampMsec.ToString(),
                    ["types"] = "DEPOSIT,WITHDRAWAL"
                });
            return result
                .Cast<dynamic>()
                .Select(x => new WalletOperationData
                {
                    DateTime = DateTimeHelper.ToDateTimeUtcMsec((long) x.date),
                    Amount = (string) x.type == "WITHDRAWAL"
                        ? -(double) x.amount
                        : (double) x.amount,
                    CurrencySymbol = (string) x.fixedCurrency,
                    Transaction = (string) x.id
                })
                .Where(x => x.DateTime > startDate)
                .ToArray();
        }

        private dynamic DoGetRequest(string relativeUrl, Dictionary<string, string> parameters = null) 
            => Api.ExecutePrivate(relativeUrl, parameters ?? new Dictionary<string, string>(), ApiKey, ApiSecret);
    }
}
