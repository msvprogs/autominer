using System;
using System.Linq;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.NetworkInfo;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public class BlockExplorerLocalWalletInfoProvider : ILocalWalletInfoProvider
    {
        private readonly string m_CoinSymbol;
        private readonly INetworkInfoProvider m_NetworkInfoProvider;

        public BlockExplorerLocalWalletInfoProvider(string coinSymbol, INetworkInfoProvider networkInfoProvider)
        {
            m_CoinSymbol = coinSymbol ?? throw new ArgumentNullException(nameof(coinSymbol));
            m_NetworkInfoProvider = networkInfoProvider ?? throw new ArgumentNullException(nameof(networkInfoProvider));
        }

        public WalletBalanceData GetBalance(string address)
        {
            var balance = m_NetworkInfoProvider.GetWalletBalance(address);
            return new WalletBalanceData
            {
                Address = address,
                Available = balance.Available,
                Unconfirmed = balance.Unconfirmed,
                CurrencySymbol = m_CoinSymbol
            };
        }

        public WalletOperationData[] GetOperations(string address, DateTime startDate) 
            => m_NetworkInfoProvider.GetWalletOperations(address, startDate)
                .Select(x => new WalletOperationData
                {
                    CurrencySymbol = m_CoinSymbol,
                    Address = address,
                    Amount = x.Amount,
                    DateTime = x.DateTime,
                    ExternalId = x.Id,
                    Transaction = x.Transaction
                })
                .ToArray();
    }
}
