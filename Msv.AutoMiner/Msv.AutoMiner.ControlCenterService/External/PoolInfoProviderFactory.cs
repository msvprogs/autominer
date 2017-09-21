using System;
using System.Linq;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.ControlCenterService.External.PoolInfoProviders;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.External
{
    public class PoolInfoProviderFactory : IPoolInfoProviderFactory
    {
        private readonly IWebClient m_WebClient;
        
        public PoolInfoProviderFactory(IWebClient webClient) 
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public IMultiPoolInfoProvider CreateMulti(PoolApiProtocol apiProtocol, string baseUrl, Pool[] pools)
        {
            if (pools == null)
                throw new ArgumentNullException(nameof(pools));
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));

            switch (apiProtocol)
            {
                case PoolApiProtocol.Yiimp:
                    return new YiimpInfoProvider(m_WebClient, baseUrl, pools);
                default:
                    throw new ArgumentOutOfRangeException(nameof(apiProtocol), "This API protocol doesn't support multiprovider");
            }
        }

        public IPoolInfoProvider Create(Pool pool)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            var miningWallet = pool.Coin.Wallets
                .FirstOrDefault(x => x.IsMiningTarget);
            switch (pool.ApiProtocol)
            {
                case PoolApiProtocol.Qwak:
                    return new QwakPoolInfoProvider(
                        m_WebClient, pool.ApiUrl, pool.PoolUserId, pool.ApiKey, pool.Coin.Algorithm.KnownValue);
                case PoolApiProtocol.Tbf:
                    return new TbfPoolInfoProvider(m_WebClient, pool.ApiUrl, pool.ApiKey);
                case PoolApiProtocol.OpenEthereum:
                    return new OpenEthereumPoolInfoProvider(m_WebClient, pool.ApiUrl, GetMiningWalletAddress());
                case PoolApiProtocol.Bitfly:
                    return new BitflyPoolInfoProvider(m_WebClient, pool.ApiUrl, GetMiningWalletAddress());
                case PoolApiProtocol.NodeOpenMiningPortal:
                    return new NodeOpenMiningPortalPoolInfoProvider(m_WebClient, pool.ApiUrl, GetMiningWalletAddress(), pool.ApiPoolName);
                case PoolApiProtocol.JsonRpcWallet:
                    return new JsonRpcLocalPoolInfoProvider(
                        new JsonRpcClient(pool.Coin.NodeHost, pool.Coin.NodePort, pool.Coin.NodeLogin, pool.Coin.NodePassword));
                default:
                    return new DummyInfoProvider();
            }

            string GetMiningWalletAddress()
            {
                if (miningWallet == null)
                    throw new InvalidOperationException($"Mining wallet for pool {pool.Name} isn't defined");
                return miningWallet.Address;
            }
        }

        private class DummyInfoProvider : IPoolInfoProvider
        {
            public PoolInfo GetInfo(DateTime minPaymentDate)
                => new PoolInfo
                {
                    AccountInfo = new PoolAccountInfo(),
                    PaymentsData = new PoolPaymentData[0],
                    State = new PoolState()
                };
        }
    }
}
