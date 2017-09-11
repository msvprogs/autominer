using System;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.External.Rpc;

namespace Msv.AutoMiner.Service.External.Network
{
    public class JsonRpcLocalNetworkInfoProvider : ICoinNetworkInfoProvider
    {
        private readonly Coin m_Coin;
        private readonly IRpcClient m_RpcClient;

        public JsonRpcLocalNetworkInfoProvider(Coin coin)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));

            m_Coin = coin;
            m_RpcClient = new JsonRpcClient(
                coin.Pool.Address, coin.Pool.Port, coin.Pool.WorkerLogin, coin.Pool.WorkerPassword);
        }

        public CoinNetworkStatistics GetNetworkStats()
        {
            switch (m_Coin.Algorithm)
            {
                default:
                    return GetBitcoinForkNetworkStatistics();
            }
        }

        private CoinNetworkStatistics GetBitcoinForkNetworkStatistics()
        {
            var info = m_RpcClient.Execute<dynamic>("getmininginfo");
            return new CoinNetworkStatistics
            {
                Height = ((long?) info.blocks).GetValueOrDefault(),
                BlockReward = (double?) info.blockvalue / 1e8,
                Difficulty = (double?) info.difficulty["proof-of-work"]
                             ?? (double?) info.difficulty["Proof of Work"]
                             ?? (double?) info.difficulty
                             ?? 0,
                NetHashRate = (long) (((double?) info.netmhashps).GetValueOrDefault() * 1e6)
            };
        }
    }
}
