using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.Exchanges.Api;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public class CryptopiaWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        public CryptopiaWalletInfoProvider(IExchangeApi api, string apiKey, string apiSecret)
            : base(api, apiKey, Convert.FromBase64String(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances() 
            => DoPostRequest<JArray>("GetBalance")
            .Cast<dynamic>()
            .Select(x => new WalletBalanceData
            {
                CurrencySymbol = (string) x.Symbol,
                Available = (double) x.Available,
                Blocked = (double) x.HeldForTrades + (double) x.PendingWithdraw,
                Unconfirmed = (double) x.Unconfirmed,
                Address = (string) x.Address
            })
            .ToArray();

        public override WalletOperationData[] GetOperations(DateTime startDate)
            => DoPostRequest<JArray>("GetTransactions")
            .Cast<dynamic>()
            .Where(x => (string) x.Status == "Confirmed")
            .Select(x => new WalletOperationData
            {
                ExternalId = (string) x.Id,
                DateTime = DateTimeHelper.Normalize((DateTime) x.Timestamp),
                Amount = (string)x.Type == "Withdraw" ? -(double) x.Amount : (double)x.Amount,
                CurrencySymbol = (string) x.Currency,
                Transaction = (string) x.TxId,
                Address = (string) x.Address
            })
            .ToArray();

        private T DoPostRequest<T>(string command)
            where T : JToken
            => (T) Api.ExecutePrivate(command, new Dictionary<string, string>(), ApiKey, ApiSecret);
    }
}
