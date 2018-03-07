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
    public class BittrexWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        public BittrexWalletInfoProvider(IExchangeApi api, string apiKey, string apiSecret)
            : base(api, apiKey, Encoding.UTF8.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
            => DoRequest<JArray>("account", "getbalances")
                .Cast<dynamic>()
                .Select(x => new WalletBalanceData
                {
                    Available = (double) x.Available,
                    Blocked = (double) x.Balance - (double) x.Available,
                    Unconfirmed = (double) x.Pending,
                    CurrencySymbol = (string) x.Currency,
                    Address = (string) x.CryptoAddress
                })
                .ToArray();

        public override WalletOperationData[] GetOperations(DateTime startDate)
            => DoRequest<JArray>("account", "getdeposithistory")
                .Cast<dynamic>()
                .Where(x => (int) x.Confirmations > 0)
                .Select(x => new WalletOperationData
                {
                    ExternalId = (string)x.PaymentUuid,
                    Amount = (double) x.Amount,
                    CurrencySymbol = (string) x.Currency,
                    DateTime = DateTimeHelper.Normalize((DateTime) x.LastUpdated),
                    Address = (string) x.Address,
                    Transaction = (string) x.TxId
                })
                .Concat(DoRequest<JArray>("account", "getwithdrawalhistory")
                    .Cast<dynamic>()
                    .Where(x => (bool) x.Authorized && !(bool) x.PendingPayment && !(bool) x.Canceled)
                    .Select(x => new WalletOperationData
                    {
                        ExternalId = (string)x.PaymentUuid,
                        Amount = -(double) x.Amount,
                        CurrencySymbol = (string) x.Currency,
                        DateTime = DateTimeHelper.Normalize((DateTime) x.Opened),
                        Address = (string) x.Address,
                        Transaction = (string) x.TxId
                    }))
                .ToArray();

        private T DoRequest<T>(string commandType, string command)
            where T : JToken 
            => (T) Api.ExecutePrivate($"{commandType}/{command}", new Dictionary<string, string>(), ApiKey, ApiSecret);
    }
}
