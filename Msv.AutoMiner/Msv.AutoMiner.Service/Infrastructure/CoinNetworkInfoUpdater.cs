using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.Infrastructure.Contracts;
using Msv.AutoMiner.Service.Storage.Contracts;
using NLog;
using Msv.AutoMiner.Commons;

namespace Msv.AutoMiner.Service.Infrastructure
{
    public class CoinNetworkInfoUpdater : ICoinNetworkInfoUpdater
    {
        private static readonly ILogger M_Logger = LogManager.GetLogger("CoinNetworkInfoUpdater");

        private readonly ICoinNetworkInfoProviderFactory m_ProviderFactory;
        private readonly IDDoSTriggerPreventingDownloader m_Downloader;
        private readonly ICoinNetworkInfoUpdaterStorage m_Storage;

        public CoinNetworkInfoUpdater(
            ICoinNetworkInfoProviderFactory providerFactory, 
            IDDoSTriggerPreventingDownloader downloader,
            ICoinNetworkInfoUpdaterStorage storage)
        {
            if (providerFactory == null)
                throw new ArgumentNullException(nameof(providerFactory));
            if (downloader == null)
                throw new ArgumentNullException(nameof(downloader));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            m_ProviderFactory = providerFactory;
            m_Downloader = downloader;
            m_Storage = storage;
        }

        public Coin[] UpdateNetworkInfo()
        {
            M_Logger.Debug("Updating coin network info...");
            var allCoins = m_Storage.GetCoins();
            var multiProvider = m_ProviderFactory.CreateMulti(allCoins, m_Downloader);
            var multiResults = new Dictionary<string, Dictionary<CoinAlgorithm, CoinNetworkStatistics>>();
            try
            {
                multiResults = multiProvider.GetMultiNetworkStats();
            }
            catch (Exception ex)
            {
                M_Logger.Error(ex, "Couldn't get network info from multiproviders");
            }

            var coins = allCoins
                .Select(x => new
                {
                    Coin = x,
                    Provider = m_ProviderFactory.Create(x)
                })
                .OrderByDescending(x => x.Coin.CurrencySymbol)
                .AsParallel()
                .WithDegreeOfParallelism(3)
                .Select(x =>
                {
                    try
                    {
                        var multiResult = multiResults.TryGetValue(x.Coin.CurrencySymbol);
                        var result = multiResult?.TryGetValue(x.Coin.Algorithm)
                            ?? multiResult?.TryGetValue(CoinAlgorithm.Unknown)
                            ?? x.Provider.GetNetworkStats();
                        if (result.NetHashRate == 0 && result.Difficulty <= 0)
                            return new { x.Coin, Result = (CoinNetworkStatistics)null };
                        var networkInfoBuilder = new StringBuilder($"New network info for {x.Coin.Name}: ")
                        .Append($"Difficulty {result.Difficulty:N4} ({GetPercentRatio(x.Coin.Difficulty, result.Difficulty)}), ")
                        .Append($"Hash Rate {ConversionHelper.ToHashRateWithUnits(result.NetHashRate, x.Coin.Algorithm)}")
                        .Append($" ({GetPercentRatio(x.Coin.NetHashRate, result.NetHashRate)})");
                        if (result.BlockTimeSeconds != null)
                            networkInfoBuilder.Append($", Current Block Time: {result.BlockTimeSeconds:F2} sec");
                        if (result.BlockReward != null)
                            networkInfoBuilder.Append($", Current Block Reward: {result.BlockReward:F4}");
                        if (result.Height != null)
                            networkInfoBuilder.Append($", Current Blockchain Height: {result.Height}");

                        M_Logger.Info(networkInfoBuilder.ToString);
                        x.Coin.Difficulty = result.Difficulty;
                        x.Coin.NetHashRate = result.NetHashRate;
                        if (result.BlockTimeSeconds != null)
                            x.Coin.BlockTimeSeconds = result.BlockTimeSeconds.Value;
                        if (result.BlockReward != null)
                            x.Coin.BlockReward = result.BlockReward.Value;
                        if (result.Height != null)
                            x.Coin.Height = result.Height.Value;
                        x.Coin.StatsUpdated = DateTime.Now;
                        return new {x.Coin, Result = result};
                    }
                    catch (Exception ex)
                    {
                        M_Logger.Error(ex, $"Couldn't get new network info for {x.Coin.Name}");
                        return new {x.Coin, Result = (CoinNetworkStatistics) null};
                    }
                })
                .Where(x => x.Result != null)
                .Select(x => x.Coin)
                .ToArray();
            M_Logger.Debug("Coin network info updated");
            m_Storage.SaveCoins(coins);
            return coins;
        }

        private static string GetPercentRatio(double oldValue, double newValue)
            => ConversionHelper.GetDiffRatioString(oldValue, newValue);
    }
}
