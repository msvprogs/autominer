using System;
using JetBrains.Annotations;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;
using Msv.AutoMiner.NetworkInfo;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public class BlockExplorerUrlProviderFactory : IBlockExplorerUrlProviderFactory
    {
        private readonly INetworkInfoProviderFactory m_NetworkInfoProviderFactory;

        public BlockExplorerUrlProviderFactory(INetworkInfoProviderFactory networkInfoProviderFactory)
        {
            m_NetworkInfoProviderFactory = networkInfoProviderFactory;
        }

        public IBlockExplorerUrlProvider Create([NotNull] Coin coin)
        {
            if (coin == null) 
                throw new ArgumentNullException(nameof(coin));

            return new ComboBlockExplorerUrlProvider(
                m_NetworkInfoProviderFactory.Create(coin),
                m_NetworkInfoProviderFactory.CreateMulti(new[] {coin}));
        }

        private class ComboBlockExplorerUrlProvider : IBlockExplorerUrlProvider
        {
            private readonly INetworkInfoProvider m_SingleProvider;
            private readonly IMultiNetworkInfoProvider m_MultiProvider;

            public ComboBlockExplorerUrlProvider(
                INetworkInfoProvider singleProvider,
                IMultiNetworkInfoProvider multiProvider)
            {
                m_SingleProvider = singleProvider;
                m_MultiProvider = multiProvider;
            }

            public Uri CreateTransactionUrl(string hash)
            {
                if (string.IsNullOrWhiteSpace(hash))
                    return null;
                return m_SingleProvider.CreateTransactionUrl(hash)
                       ?? m_MultiProvider.CreateTransactionUrl(hash);
            }

            public Uri CreateAddressUrl(string address)
            {
                if (string.IsNullOrWhiteSpace(address))
                    return null;
                return m_SingleProvider.CreateAddressUrl(address)
                       ?? m_MultiProvider.CreateAddressUrl(address);
            }


            public Uri CreateBlockUrl(string blockHash)
            {
                if (string.IsNullOrWhiteSpace(blockHash))
                    return null;
                return m_SingleProvider.CreateBlockUrl(blockHash)
                       ?? m_MultiProvider.CreateBlockUrl(blockHash);
            }
        }
    }
}
