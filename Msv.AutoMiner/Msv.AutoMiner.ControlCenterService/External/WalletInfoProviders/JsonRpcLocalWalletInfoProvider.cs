using System;
using System.Linq;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.Data;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public class JsonRpcLocalWalletInfoProvider : IWalletInfoProvider
    {
        private const int LastTransactionCount = 40;

        private readonly IRpcClient m_RpcClient;
        private readonly Coin m_Coin;

        public JsonRpcLocalWalletInfoProvider(IRpcClient rpcClient, Coin coin)
        {
            m_RpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
            m_Coin = coin ?? throw new ArgumentNullException(nameof(coin));
        }

        public WalletBalanceData[] GetBalances()
        {
            var info = m_RpcClient.Execute<dynamic>("getinfo");
            return new[]
            {
                new WalletBalanceData
                {
                    Available = (double) info.balance,
                    Unconfirmed = (double) info.newmint,
                    CurrencySymbol = m_Coin.Symbol
                }
            };
        }

        public WalletOperationData[] GetOperations(DateTime startDate)
            => m_RpcClient.Execute<JArray>("listtransactions", string.Empty, LastTransactionCount)
                .Cast<dynamic>()
                .Where(x => (int) x.confirmations > 0)
                .Select(x => new WalletOperationData
                {
                    Address = (string) x.address,
                    Amount = (double) x.amount + ((double?) x.fee).GetValueOrDefault(),
                    CurrencySymbol = m_Coin.Symbol,
                    DateTime = DateTimeHelper.ToDateTimeUtc((long) x.time),
                    Transaction = (string) x.txid
                })
                .ToArray();
    }
}
