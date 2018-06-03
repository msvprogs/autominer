using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Msv.AutoMiner.CoinInfoService.Infrastructure;
using Msv.AutoMiner.CoinInfoService.Logic.Profitability;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic.Contracts;
using Msv.AutoMiner.NetworkInfo;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Monitors
{
    public class NetworkInfoMonitor : MonitorBase
    {       
        private const int ProviderParallelismDegree = 6;
        // If none of actual coinbase tx outputs differs less than 5% from the calculated reward value, warn the user.
        private const double BlockRewardAccuracyPercent = 5;

        private static readonly TimeSpan M_MaxLastBlockPastDifference = TimeSpan.FromHours(3);
        private static readonly TimeSpan M_MaxLastBlockFutureDifference = TimeSpan.FromMinutes(15);

        private readonly IBlockRewardCalculator m_RewardCalculator;
        private readonly ICoinNetworkInfoProvider m_StoredInfoProvider;
        private readonly INetworkInfoProviderFactory m_ProviderFactory;
        private readonly IMasternodeInfoStorage m_MasternodeInfoStorage;
        private readonly INetworkInfoMonitorStorage m_Storage;

        public NetworkInfoMonitor(
            IBlockRewardCalculator blockRewardCalculator,
            ICoinNetworkInfoProvider storedInfoProvider,
            INetworkInfoProviderFactory providerFactory,
            IMasternodeInfoStorage masternodeInfoStorage,
            INetworkInfoMonitorStorage storage) 
            : base(TimeSpan.FromMinutes(15))
        {
            m_RewardCalculator = blockRewardCalculator ?? throw new ArgumentNullException(nameof(blockRewardCalculator));
            m_StoredInfoProvider = storedInfoProvider ?? throw new ArgumentNullException(nameof(storedInfoProvider));
            m_ProviderFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            m_MasternodeInfoStorage = masternodeInfoStorage ?? throw new ArgumentNullException(nameof(masternodeInfoStorage));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        protected override void DoWork()
        {
            var coins = m_Storage.GetCoins();
            var multiProviderCoins = coins
                .Where(x => x.NetworkInfoApiType == CoinNetworkInfoApiType.SpecialMulti)
                .ToArray();

            var multiProvider = m_ProviderFactory.CreateMulti(multiProviderCoins);
            var multiProviderResults = multiProviderCoins.Any() 
                ? multiProvider.GetMultiNetworkStats() 
                : new Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>>();

            var previousInfos = m_StoredInfoProvider.GetCurrentNetworkInfos(true)
                .ToDictionary(x => x.CoinId);
            var now = DateTime.UtcNow;
            var random = new Random();
            coins.Where(x => x.NetworkInfoApiType != CoinNetworkInfoApiType.NoApi)
                .OrderBy(x => random.NextDouble())
                .AsParallel()
                .WithDegreeOfParallelism(ProviderParallelismDegree)
                .Select(x => TryProcessSingleCoin(x, multiProviderResults, previousInfos))
                .Where(x => x.result != null)
                .Select(x => new CoinNetworkInfo
                {
                    CoinId = x.coin.Id,
                    Created = now,
                    BlockReward = x.result.BlockReward.NullIfNaN().GetValueOrDefault(),
                    BlockTimeSeconds = x.result.BlockTimeSeconds.NullIfNaN().GetValueOrDefault(),
                    Difficulty = x.result.Difficulty.ZeroIfNaN(),
                    Height = x.result.Height,
                    NetHashRate = x.result.NetHashRate.ZeroIfNaN(),
                    LastBlockTime = x.result.LastBlockTime,
                    MasternodeCount = x.result.MasternodeCount,
                    TotalSupply = x.result.TotalSupply.NullIfNaN()
                })
                .ForAll(x => m_Storage.StoreNetworkInfo(x));
        }

        private (Coin coin, CoinNetworkStatistics result) TryProcessSingleCoin(Coin coin,
            Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>> multiProviderResults,
            Dictionary<Guid, CoinNetworkInfo> previousInfos)
        {
            try
            {
                var result = ProcessSingleCoin(coin, multiProviderResults, previousInfos);
                if (result.coin.DisableBlockRewardChecking 
                    || result.result.BlockReward == null 
                    || result.result.LastBlockTransactions.IsNullOrEmpty())
                {
                    m_Storage.StoreCoinNetworkResult(coin.Id, CoinLastNetworkInfoResult.Success, "Block reward isn't verified");
                    return result;
                }
                var coinbase = result.result.LastBlockTransactions
                    .FirstOrDefault(x => x.InValues.IsNullOrEmpty());
                if (coinbase == null)
                {
                    m_Storage.StoreCoinNetworkResult(coin.Id, CoinLastNetworkInfoResult.Success, null);
                    return result;
                }

                // Check that any output of the coinbase tx equals the calculated block reward.
                // It doesn't give the guarantee that calculated value is correct,
                // but is should detect the most part of the sudden block reward calculation algorithm changes.
                var fees = result.result.LastBlockTransactions
                    .Where(x => x != coinbase)
                    .Select(x => x.Fee ?? x.InValues.Sum() - x.OutValues.Sum())
                    .Sum();
                var totalCoinbaseOutput = coinbase.OutValues.Sum();
                // Assume that mining fees are distributed equally among all outputs
                // Potential errors of this assumption should be passed by tweaking BlockRewardAccuracyPercent
                if (!coinbase.OutValues
                    .Select(x => x - x/totalCoinbaseOutput*fees)
                    .Any(x => Math.Abs(ConversionHelper.GetDiffRatio(x, result.result.BlockReward.Value)) <= BlockRewardAccuracyPercent))
                {
                    m_Storage.StoreCoinNetworkResult(coin.Id, CoinLastNetworkInfoResult.BlockRewardMismatch, 
                        $"Expected reward: {result.result.BlockReward:N4}, " 
                        + $"actual coinbase out values: {string.Join(", ", coinbase.OutValues.Select(x => x.ToString("N4")))}, "
                        + $"mining fees {fees:N8}");
                    return result;
                }

                m_Storage.StoreCoinNetworkResult(coin.Id, CoinLastNetworkInfoResult.Success, null);
                return result;
            }
            catch (NoPoWBlocksException noPoWEx)
            {
                Log.Error($"{coin.Name}: {noPoWEx.Message}");
                m_Storage.StoreCoinNetworkResult(coin.Id, CoinLastNetworkInfoResult.NoPoWBlocks, noPoWEx.Message);
                return (coin, result: null);
            }
            catch (BlockchainOutOfSyncException outOfSyncEx)
            {
                Log.Error($"{coin.Name}: {outOfSyncEx.Message}");
                m_Storage.StoreCoinNetworkResult(coin.Id, CoinLastNetworkInfoResult.OutOfSync, outOfSyncEx.Message);
                return (coin, result: null);
            }
            catch (HttpRequestException httpEx)
            {
                Log.Error(httpEx, $"Couldn't get new network info for {coin.Name}");
                var message = httpEx.InnerException?.Message ?? httpEx.Message;
                m_Storage.StoreCoinNetworkResult(coin.Id, CoinLastNetworkInfoResult.Exception, message);
                return (coin, result: null);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Couldn't get new network info for {coin.Name}");
                m_Storage.StoreCoinNetworkResult(coin.Id, CoinLastNetworkInfoResult.Exception, ex.Message);
                return (coin, result: null);
            }
        }

        private (Coin coin, CoinNetworkStatistics result) ProcessSingleCoin(
            Coin coin, 
            Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>> multiProviderResults,
            Dictionary<Guid, CoinNetworkInfo> previousInfos)
        {
            var provider = m_ProviderFactory.Create(coin);
            var multiProviderResult = multiProviderResults.TryGetValue(coin.Symbol);
            var result = multiProviderResult?.TryGetValue(coin.Algorithm.KnownValue.GetValueOrDefault())
                         ?? multiProviderResult?.TryGetValue(KnownCoinAlgorithm.Unknown)
                         ?? provider.GetNetworkStats();
            if (result.NetHashRate <= 0 && result.Difficulty <= 0)
                return (coin, result: null);

            var masternodeInfo = m_MasternodeInfoStorage.Load(coin.Symbol);
            if (result.MasternodeCount == null)
                result.MasternodeCount = masternodeInfo?.MasternodesCount;
            if (result.TotalSupply == null)
                result.TotalSupply = masternodeInfo?.TotalSupply;

            if (!string.IsNullOrWhiteSpace(coin.RewardCalculationJavaScript))
            {
                var reward = m_RewardCalculator.Calculate(
                    coin.RewardCalculationJavaScript, result.Height, result.Difficulty, result.TotalSupply, result.MasternodeCount);
                if (reward != null)
                    result.BlockReward = reward;
            }

            LogResults(coin, result, previousInfos.TryGetValue(coin.Id, new CoinNetworkInfo()));
            if (result.LastBlockTime.HasValue
                && (DateTime.UtcNow - result.LastBlockTime.Value > M_MaxLastBlockPastDifference
                    || result.LastBlockTime.Value - DateTime.UtcNow > M_MaxLastBlockFutureDifference))
                throw new BlockchainOutOfSyncException(result.LastBlockTime.Value);
            return (coin, result);
        }

        private void LogResults(Coin coin, CoinNetworkStatistics current, CoinNetworkInfo previous)
        {
            var networkInfoBuilder = new StringBuilder($"New network info for {coin.Name}: ")
                .Append($"Difficulty {current.Difficulty:N4} ({ConversionHelper.GetDiffRatioString(previous.Difficulty, current.Difficulty)}), ")
                .Append($"Hash Rate {ConversionHelper.ToHashRateWithUnits(current.NetHashRate, coin.Algorithm.KnownValue)}")
                .Append($" ({ConversionHelper.GetDiffRatioString(current.NetHashRate, previous.NetHashRate)})");
            if (current.BlockTimeSeconds != null)
                networkInfoBuilder.Append($", Current Block Time: {current.BlockTimeSeconds:F2} sec");
            if (current.BlockReward != null)
                networkInfoBuilder.Append($", Current Block Reward: {current.BlockReward:F4}");
            networkInfoBuilder.Append($", Current Blockchain Height: {current.Height}");
            if (current.LastBlockTime != null)
                networkInfoBuilder.Append($", Last Block Time: {current.LastBlockTime.Value:R}");
            if (current.MasternodeCount != null)
                networkInfoBuilder.Append($", Masternodes: {current.MasternodeCount}");
            if (current.TotalSupply != null)
                networkInfoBuilder.Append($", Total Supply: {current.TotalSupply:N0}");
            Log.Info(networkInfoBuilder.ToString());
        }
    }
}
