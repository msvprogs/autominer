using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Msv.AutoMiner.CoinInfoService.Logic.Profitability;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.NetworkInfo;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Monitors
{
    public class NetworkInfoMonitor : MonitorBase
    {       
        private const int ProviderParallelismDegree = 6;

        private static readonly TimeSpan M_MaxLastBlockPastDifference = TimeSpan.FromHours(3);
        private static readonly TimeSpan M_MaxLastBlockFutureDifference = TimeSpan.FromMinutes(15);

        private readonly IBlockRewardCalculator m_RewardCalculator;
        private readonly ICoinNetworkInfoProvider m_StoredInfoProvider;
        private readonly INetworkInfoProviderFactory m_ProviderFactory;
        private readonly INetworkInfoMonitorStorage m_Storage;

        public NetworkInfoMonitor(
            IBlockRewardCalculator blockRewardCalculator,
            ICoinNetworkInfoProvider storedInfoProvider,
            INetworkInfoProviderFactory providerFactory,
            INetworkInfoMonitorStorage storage) 
            : base(TimeSpan.FromMinutes(15))
        {
            m_RewardCalculator = blockRewardCalculator ?? throw new ArgumentNullException(nameof(blockRewardCalculator));
            m_StoredInfoProvider = storedInfoProvider ?? throw new ArgumentNullException(nameof(storedInfoProvider));
            m_ProviderFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        protected override void DoWork()
        {
            var coins = m_Storage.GetCoins();
            var multiProviderCoins = coins
                .Where(x => x.NetworkInfoApiType == CoinNetworkInfoApiType.SpecialMulti)
                .ToArray();

            var multiProvider = m_ProviderFactory.CreateMulti(multiProviderCoins);
            var multiResults = multiProviderCoins.Any() 
                ? multiProvider.GetMultiNetworkStats() 
                : new Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>>();

            var previousInfos = m_StoredInfoProvider.GetCurrentNetworkInfos(true)
                .ToDictionary(x => x.CoinId);
            var now = DateTime.UtcNow;
            var random = new Random();
            coins.OrderBy(x => random.NextDouble())
                .AsParallel()
                .WithDegreeOfParallelism(ProviderParallelismDegree)
                .Select(x =>
                {
                    try
                    {
                        var provider = m_ProviderFactory.Create(x);
                        var multiResult = multiResults.TryGetValue(x.Symbol);
                        var result = multiResult?.TryGetValue(x.Algorithm.KnownValue.GetValueOrDefault())
                                     ?? multiResult?.TryGetValue(KnownCoinAlgorithm.Unknown)
                                     ?? provider.GetNetworkStats();
                        if (result.NetHashRate <= 0 && result.Difficulty <= 0)
                            return (coin: x, result: null);
                        if (!string.IsNullOrWhiteSpace(x.RewardCalculationJavaScript))
                        {
                            var reward = m_RewardCalculator.Calculate(
                                x.RewardCalculationJavaScript, result.Height, result.Difficulty, result.MoneySupply, result.MasternodeCount);
                            if (reward != null)
                                result.BlockReward = reward;
                        }
                        LogResults(x, result, previousInfos.TryGetValue(x.Id, new CoinNetworkInfo()));
                        if (result.LastBlockTime.HasValue
                            && (DateTime.UtcNow - result.LastBlockTime.Value > M_MaxLastBlockPastDifference
                            || result.LastBlockTime.Value - DateTime.UtcNow > M_MaxLastBlockFutureDifference))
                            throw new ExternalDataUnavailableException(
                                $"Provider blockchain is possibly out of sync: last block time is {result.LastBlockTime:R}, current time is {DateTime.UtcNow:R}");
                        return (coin: x, result);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Couldn't get new network info for {x.Name}");
                        return (coin: x, result: null);
                    }
                })
                .Where(x => x.result != null)
                .Select(x => new CoinNetworkInfo
                {
                    CoinId = x.coin.Id,
                    Created = now,
                    BlockReward = x.result.BlockReward.NullIfNaN() ?? 0,
                    BlockTimeSeconds = x.coin.CanonicalBlockTimeSeconds.NullIfNaN()
                                       ?? x.result.BlockTimeSeconds.NullIfNaN()
                                       ?? 0,
                    Difficulty = x.result.Difficulty.ZeroIfNaN(),
                    Height = x.result.Height,
                    NetHashRate = x.result.NetHashRate.ZeroIfNaN(),
                    LastBlockTime = x.result.LastBlockTime
                })
                .ForAll(x => m_Storage.StoreNetworkInfo(x));
        }

        private void LogResults(Coin coin, CoinNetworkStatistics current, CoinNetworkInfo previous)
        {
            var networkInfoBuilder = new StringBuilder($"New network info for {coin.Name}: ")
                .Append($"Difficulty {current.Difficulty:N4} ({GetPercentRatio(previous.Difficulty, current.Difficulty)}), ")
                .Append($"Hash Rate {ConversionHelper.ToHashRateWithUnits(current.NetHashRate, coin.Algorithm.KnownValue)}")
                .Append($" ({GetPercentRatio(current.NetHashRate, previous.NetHashRate)})");
            if (current.BlockTimeSeconds != null)
                networkInfoBuilder.Append($", Current Block Time: {current.BlockTimeSeconds:F2} sec");
            if (current.BlockReward != null)
                networkInfoBuilder.Append($", Current Block Reward: {current.BlockReward:F4}");
            networkInfoBuilder.Append($", Current Blockchain Height: {current.Height}");
            if (current.LastBlockTime != null)
                networkInfoBuilder.Append($", Last Block Time: {current.LastBlockTime.Value:R}");
            Log.Info(networkInfoBuilder.ToString());
        }

        private static string GetPercentRatio(double oldValue, double newValue)
            => ConversionHelper.GetDiffRatioString(oldValue, newValue);
    }
}
