using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Data;
using NLog;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    public class JsonRpcLocalNetworkInfoProvider : INetworkInfoProvider
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly IRpcClient m_RpcClient;
        private readonly Coin m_Coin;
        private readonly INetworkInfoProvider m_FallbackProvider;

        public JsonRpcLocalNetworkInfoProvider(IRpcClient rpcClient, Coin coin, INetworkInfoProvider fallbackProvider)
        {
            m_RpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
            m_Coin = coin ?? throw new ArgumentNullException(nameof(coin));
            m_FallbackProvider = fallbackProvider;
        }

        public CoinNetworkStatistics GetNetworkStats()
        {
            try
            {
                return GetRpcNetworkStats();
            }
            catch (Exception ex) when (m_FallbackProvider != null)
            {
                M_Logger.Error(ex, $"Local wallet for {m_Coin.Symbol} is unavailable, trying fallback provider");
                return m_FallbackProvider.GetNetworkStats();
            }
        }

        private CoinNetworkStatistics GetRpcNetworkStats()
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