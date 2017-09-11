using System;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.External.Pools;

namespace Msv.AutoMiner.Service.External
{
    public class PoolInfoProviderFactory : IPoolInfoProviderFactory
    {
        public IPoolInfoProvider Create(Coin coin, IDDoSTriggerPreventingDownloader downloader)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));
            var pool = coin.Pool;
            switch (pool.ApiProtocol)
            {
                case PoolApiProtocol.Qwak:
                    return new QwakPoolInfoProvider(pool.ApiUrl, pool.PoolUserId, pool.ApiKey, coin.Algorithm);
                case PoolApiProtocol.Tbf:
                    return new TbfPoolInfoProvider(pool.ApiUrl, pool.ApiKey);
                case PoolApiProtocol.OpenEthereum:
                    return new OpenEthereumPoolInfoProvider(pool.ApiUrl, coin.Wallet);
                case PoolApiProtocol.Bitfly:
                    return new BitflyPoolInfoProvider(pool.ApiUrl, coin.Wallet);
                case PoolApiProtocol.NodeOpenMiningPortal:
                    return new NodeOpenMiningPortalPoolInfoProvider(pool.ApiUrl, coin.Wallet, pool.ApiPoolName);
                case PoolApiProtocol.Yiimp:
                    return new YiimpInfoProvider(downloader, pool.ApiUrl, pool.IsAnonymous ? coin.Wallet : pool.WorkerLogin, pool.ApiPoolName);
                case PoolApiProtocol.JsonRpcWallet:
                    return new JsonRpcWalletInfoProvider(pool);
               // case PoolApiProtocol.XpmForAll:
                //    return new XpmForAllPoolInfoProvider(pool.ApiUrl, coin.Wallet);
                default:
                    return new DummyInfoProvider();
            }
        }

        private class DummyInfoProvider : IPoolInfoProvider
        {
            public PoolInfo GetInfo(DateTime minPaymentDate)
                => new PoolInfo(new PoolAccountInfo(), new PoolState(), new PoolPaymentData[0]);
        }
    }
}
