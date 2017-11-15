using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Monitors
{
    public class NetworkInfoMonitor : MonitorBase
    {
        private const int ProviderParallelismDegree = 3;
        private readonly INetworkInfoProviderFactory m_ProviderFactory;
        private readonly Func<INetworkInfoMonitorStorage> m_StorageGetter;

        public NetworkInfoMonitor(INetworkInfoProviderFactory providerFactory, Func<INetworkInfoMonitorStorage> storageGetter) 
            : base(TimeSpan.FromMinutes(10))
        {
            m_ProviderFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            m_StorageGetter = storageGetter ?? throw new ArgumentNullException(nameof(storageGetter));
        }

        protected override void DoWork()
        {
            var storage = m_StorageGetter.Invoke();
            var coins = storage.GetCoins();
            var multiProviderCoins = coins
                .Where(x => x.NetworkInfoApiType == CoinNetworkInfoApiType.Zpool)
                .ToArray();

            var previousInfos = storage.GetLastNetworkInfos()
                .ToDictionary(x => x.CoinId);
#if !DEBUG
            if (previousInfos.Select(x => x.Value.Created)
                .OrderByDescending(x => x)
                .FirstOrDefault() > DateTime.UtcNow - Period)
                return;
#endif

            var multiProvider = m_ProviderFactory.CreateMulti(multiProviderCoins);
            var multiResults = new Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>>();
            try
            {
                if (multiProviderCoins.Any())
                    multiResults = multiProvider.GetMultiNetworkStats();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Couldn't get network info from multiproviders");
            }       

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
                        LogResults(x, result, previousInfos.TryGetValue(x.Id, new CoinNetworkInfo()));
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
                    BlockReward = x.coin.CanonicalBlockReward ?? x.result.BlockReward ?? 0,
                    BlockTimeSeconds = x.coin.CanonicalBlockTimeSeconds ?? x.result.BlockTimeSeconds ?? 0,
                    Difficulty = x.result.Difficulty,
                    Height = x.result.Height,
                    NetHashRate = x.result.NetHashRate
                })
                .ForEach(x => storage.StoreNetworkInfo(x));
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
            Log.Info(networkInfoBuilder.ToString());
        }

        private static string GetPercentRatio(double oldValue, double newValue)
            => ConversionHelper.GetDiffRatioString(oldValue, newValue);
    }
}
