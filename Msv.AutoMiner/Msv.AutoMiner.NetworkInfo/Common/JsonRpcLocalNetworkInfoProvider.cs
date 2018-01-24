using System;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.NetworkInfo.Data;
using Msv.AutoMiner.NetworkInfo.Utilities;
using Newtonsoft.Json.Linq;
using NLog;

namespace Msv.AutoMiner.NetworkInfo.Common
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

        public Uri CreateTransactionUrl(string hash)
            => m_FallbackProvider?.CreateTransactionUrl(hash);

        public Uri CreateAddressUrl(string address)
            => m_FallbackProvider?.CreateAddressUrl(address);

        public Uri CreateBlockUrl(string blockHash)
            => m_FallbackProvider?.CreateBlockUrl(blockHash);

        private CoinNetworkStatistics GetRpcNetworkStats()
        {
            var info = m_RpcClient.Execute<dynamic>("getmininginfo");
            var bestBlockInfo = m_RpcClient.Execute<BlockHeader>(
                "getblock", m_RpcClient.Execute<string>("getbestblockhash"));

            return new CoinNetworkStatistics
            {
                Height = (long?) info.blocks ?? 0,
                BlockReward = (double?) info.blockvalue / 1e8,
                Difficulty = m_Coin.GetDifficultyFromLastPoWBlock
                    ? new BlockChainSearcher(x => m_RpcClient.Execute<BlockHeader>("getblock", x))
                        .SearchPoWBlock(bestBlockInfo).Difficulty
                    : ParseDifficulty(info),
                NetHashRate = (long?) ((double?) info.netmhashps * 1e6)
                              ?? (long?) info.networkhashps
                              ?? 0,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc(bestBlockInfo.Time)
            };
        }

        private static double ParseDifficulty(dynamic miningInfoJson)
        {
            var difficulty = miningInfoJson.difficulty is JObject difficultyObj
                ? ((double?) difficultyObj["proof-of-work"]
                   ?? (double?) difficultyObj["Proof of Work"])
                : (double?) miningInfoJson.difficulty;
            if (difficulty == null)
                throw new ExternalDataUnavailableException("Couldn't parse difficulty from getmininginfo() response");
            return difficulty.Value;
        }
    }
}