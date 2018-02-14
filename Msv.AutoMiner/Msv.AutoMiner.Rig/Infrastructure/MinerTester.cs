using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Rig.Data;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;
using Msv.AutoMiner.Rig.Remote;
using Msv.AutoMiner.Rig.Storage.Contracts;
using Msv.AutoMiner.Rig.Storage.Model;
using Msv.AutoMiner.Rig.System.Video;
using NLog;
using AlgorithmData = Msv.AutoMiner.Rig.Storage.Model.AlgorithmData;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class MinerTester : IMinerTester
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly IMinerProcessController m_Controller;
        private readonly IVideoSystemStateProvider m_VideoStateProvider;
        private readonly IControlCenterService m_ControlCenterService;
        private readonly IMinerTesterStorage m_Storage;
        private readonly TimeSpan m_TestDuration;

        public MinerTester(
            IMinerProcessController controller,
            IVideoSystemStateProvider videoStateProvider,
            IControlCenterService controlCenterService,
            IMinerTesterStorage storage,
            TimeSpan testDuration)
        {
            m_Controller = controller ?? throw new ArgumentNullException(nameof(controller));
            m_VideoStateProvider = videoStateProvider ?? throw new ArgumentNullException(nameof(videoStateProvider));
            m_ControlCenterService = controlCenterService ?? throw new ArgumentNullException(nameof(controlCenterService));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            m_TestDuration = testDuration;
        }

        public void Test(string[] algorithmNames, string[] coinNames)
        {
            M_Logger.Info("Getting coin info from server...");
            var algorithmSettings = m_Storage.GetMinerAlgorithmSettings()
                .Where(x => algorithmNames == null
                            || algorithmNames.Contains(x.Algorithm.AlgorithmName,
                                StringComparer.InvariantCultureIgnoreCase))
                .ToDictionary(x => Guid.Parse(x.AlgorithmId));
            var algorithmsWithCoins = m_ControlCenterService.GetMiningWork(
                    new GetMiningWorkRequestModel
                    {
                        TestMode = true,
                        AlgorithmDatas = algorithmSettings.Keys
                            .Select(x => new AlgorithmPowerData
                            {
                                AlgorithmId = x
                            })
                            .ToArray()
                    })
                .Where(x => coinNames == null
                            || coinNames.Contains(x.CoinName, StringComparer.InvariantCultureIgnoreCase)
                            || coinNames.Contains(x.CoinSymbol, StringComparer.InvariantCultureIgnoreCase))
                .GroupBy(x => x.CoinAlgorithmId)
                .Select(x => new
                {
                    Coin = x.FirstOrDefault(y => y.Pools.Any()),
                    MinerSetting = algorithmSettings.TryGetValue(x.Key),
                    AlgorithmId = x.Key
                })
                .Where(x => x.Coin != null && x.MinerSetting != null)
                .ToDictionary(x => x.AlgorithmId, x => (coin:x.Coin, settings:x.MinerSetting));
            var algorithmsWithoutCoins = coinNames == null
                ? algorithmSettings
                    .Where(x => !algorithmsWithCoins.ContainsKey(x.Key))
                    .ToDictionary(x => x.Key, x => (coin: (MiningWorkModel) null, settings:x.Value))
                : new Dictionary<Guid, (MiningWorkModel coin, MinerAlgorithmSetting settings)>();
            M_Logger.Info("Algorithms with coins: "
                          + string.Join(", ",
                              algorithmsWithCoins.Select(x => x.Value.settings.Algorithm.AlgorithmName)
                                  .OrderBy(x => x)));
            M_Logger.Info("Algorithms without coins (offline benchmark): "
                          + string.Join(", ",
                              algorithmsWithoutCoins.Select(x => x.Value.settings.Algorithm.AlgorithmName)
                                  .OrderBy(x => x)));

            var results = algorithmsWithCoins.Values
                .Concat(algorithmsWithoutCoins.Values)
                .OrderBy(x => x.settings.Algorithm.AlgorithmName)
                .Select((x, i) => TestSingle(x.coin, x.settings, i + 1,
                    algorithmsWithCoins.Count + algorithmsWithoutCoins.Count))
                .ToArray();

            M_Logger.Info("Test results: "
                          + Environment.NewLine
                          + string.Join(Environment.NewLine,
                              results.Select(x => $"{x.Algorithm.AlgorithmName}: {(x.IsSuccess ? "OK" : "Fail")}, "
                                                  + $" hashrate {ConversionHelper.ToHashRateWithUnits(x.HashRate, x.Algorithm.KnownValue)},"
                                                  + $" power usage {x.PowerUsage:F2} W")));
        }

        private TestResult TestSingle(MiningWorkModel coin, MinerAlgorithmSetting settings, int index, int total)
        {
            var algorithm = settings.Algorithm;
            M_Logger.Info(
                $"Testing {algorithm.AlgorithmName} ({(coin != null ? coin.CoinName : "<offline>")}) [{index} / {total}]...");
            var result = new TestResult
            {
                Algorithm = algorithm
            };
            if (coin == null
                && (settings.Miner.BenchmarkArgument == null
                || settings.Miner.BenchmarkResultRegex == null))
            {
                M_Logger.Warn(
                    $"{algorithm.AlgorithmName}: no active coins available, and offline benchmark is not supported");
                return result;
            }
            try
            {
                m_Controller.RunNew(new CoinMiningData
                {
                    CoinId = coin?.CoinId ?? default,
                    CoinName = coin?.CoinName,
                    CoinSymbol = coin?.CoinSymbol,
                    MinerSettings = settings,
                    BenchmarkMode = true,
                    PoolData = coin?.Pools.FirstOrDefault()
                });
                M_Logger.Info($"Waiting {m_TestDuration.TotalMinutes:F2} minutes...");
                var powerUsages = new List<decimal>();
                using (Observable.Interval(TimeSpan.FromSeconds(10))
                    .Select(x => m_VideoStateProvider.CanUse ? m_VideoStateProvider.GetState() : null)
                    .Where(x => x != null && x.AdapterStates?.Length > 0)
                    .Subscribe(x => powerUsages.Add(x.AdapterStates.Sum(y => y.PowerUsage))))
                {
                    Thread.Sleep(m_TestDuration);
                }
                var hashRate = m_Controller.CurrentState.CurrentHashRate;
                m_Controller.Stop();

                if (hashRate <= 0)
                {
                    M_Logger.Error("FAIL: Something is wrong, because hashrate is zero");
                    result.IsSuccess = false;
                    return result;
                }
                M_Logger.Info(
                    $"SUCCESS: Current hashrate of {algorithm.AlgorithmName} is {ConversionHelper.ToHashRateWithUnits(hashRate, algorithm.KnownValue)}");
                result.IsSuccess = true;
                result.HashRate = hashRate;
                result.PowerUsage = Math.Round((double) powerUsages.DefaultIfEmpty().Average(), 2);
                M_Logger.Info("Storing hashrate in DB");
                m_Storage.StoreAlgorithmData(Guid.Parse(algorithm.AlgorithmId),
                    algorithm.AlgorithmName, hashRate, result.PowerUsage);
            }
            catch (Exception ex)
            {
                M_Logger.Error(ex, "FAIL: Test failed with exception");
                result.IsSuccess = false;
            }
            return result;
        }

        private class TestResult
        {
            public bool IsSuccess { get; set; }
            public double HashRate { get; set; }
            public double PowerUsage { get; set; }
            public AlgorithmData Algorithm { get; set; }
        }
    }
}
