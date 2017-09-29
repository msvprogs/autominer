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
using NLog;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class MinerTester
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly IMinerProcessController m_Controller;
        private readonly IVideoAdapterMonitor m_VideoAdapterMonitor;
        private readonly IControlCenterService m_ControlCenterService;
        private readonly IMinerTesterStorage m_Storage;
        private readonly TimeSpan m_TestDuration;
        private readonly string[] m_Algorithms;

        public MinerTester(
            IMinerProcessController controller,
            IVideoAdapterMonitor videoAdapterMonitor,
            IControlCenterService controlCenterService,
            IMinerTesterStorage storage,
            TimeSpan testDuration,
            string[] algorithms)
        {
            m_Controller = controller ?? throw new ArgumentNullException(nameof(controller));
            m_VideoAdapterMonitor = videoAdapterMonitor ?? throw new ArgumentNullException(nameof(videoAdapterMonitor));
            m_ControlCenterService = controlCenterService ?? throw new ArgumentNullException(nameof(controlCenterService));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            m_TestDuration = testDuration;
            m_Algorithms = algorithms;
        }

        public void Test(bool benchmarkMode)
        {
            M_Logger.Info("Running miner tests (only for active coins with pools)...");

            var algorithms = m_ControlCenterService.GetAlgorithms()
                .Where(x => m_Algorithms == null || m_Algorithms.Contains(x.Name, StringComparer.InvariantCultureIgnoreCase))
                .ToDictionary(x => x.Id);
            M_Logger.Info($"Got {algorithms.Count} algorithms from server");

            var algorithmSettings = m_Storage.GetMinerAlgorithmSettings()
                .ToDictionary(x => x.AlgorithmId);
            var coins = m_ControlCenterService.GetMiningWork(
                new GetMiningWorkRequestModel
                {
                    TestMode = true,
                    AlgorithmDatas = algorithms.Values
                        .Select(x => new AlgorithmPowerData
                        {
                            AlgorithmId = x.Id
                        })
                        .ToArray()
                })
                .Where(x => x.Pools.Any())
                .Select(x => new
                {
                    Algorithm = x.CoinAlgorithmId,
                    Coin = x,
                    MinerSetting = algorithmSettings.TryGetValue(x.CoinAlgorithmId)
                })
                .Where(x => x.MinerSetting != null)
                .ToArray();
            M_Logger.Info($"Got {coins.Length} coins from server");

            if (benchmarkMode)
                coins = coins
                    .GroupBy(x => x.Algorithm)
                    .Select(x => new
                    {
                        Algorithm = x.Key,
                        x.First().Coin,
                        MinerSetting = algorithmSettings.TryGetValue(x.Key)
                    })
                    .ToArray();

            var testedAlgorithms = new List<AlgorithmInfo>();
            var results = new List<TestResult>();
            foreach (var coinGroup in coins)
            {
                var algorithm = algorithms[coinGroup.Algorithm];
                M_Logger.Info(
                    $"Testing {coinGroup.Coin.CoinName} [{algorithms[coinGroup.Algorithm].Name}]...");
                var result = new TestResult
                {
                    Symbol = coinGroup.Coin.CoinSymbol,
                    Algorithm = algorithm
                };
                try
                {
                    m_Controller.RunNew(new CoinMiningData
                    {
                        CoinId = coinGroup.Coin.CoinId,
                        CoinName = coinGroup.Coin.CoinName,
                        CoinSymbol = coinGroup.Coin.CoinSymbol,
                        MinerSettings = coinGroup.MinerSetting,
                        PoolData = coinGroup.Coin.Pools.First()
                    });
                    M_Logger.Info($"Waiting {m_TestDuration.TotalMinutes:F2} minutes...");
                    var powerUsages = new List<decimal>();
                    using (Observable.Interval(TimeSpan.FromSeconds(10))
                        .Select(x => m_VideoAdapterMonitor.GetCurrentState())
                        .Where(x => x != null && x.AdapterStates?.Length > 0)
                        .Subscribe(x => powerUsages.Add(x.AdapterStates.Sum(y => y.PowerUsage))))
                    {
                        Thread.Sleep(m_TestDuration);
                    }
                    var hashRate = m_Controller.CurrentState.CurrentHashRate;
                    m_Controller.Stop();

                    if (hashRate == 0)
                    {
                        M_Logger.Error("FAIL: Something is wrong, because hashrate is zero");
                        result.IsSuccess = false;
                        results.Add(result);
                        continue;
                    }
                    M_Logger.Info(
                        $"SUCCESS: Current hashrate of {algorithm.Name} is {ConversionHelper.ToHashRateWithUnits(hashRate, algorithm.KnownValue)}");
                    result.IsSuccess = true;
                    result.HashRate = hashRate;
                    result.PowerUsage = Math.Round((double) powerUsages.DefaultIfEmpty().Average(), 2);
                    results.Add(result);
                    if (testedAlgorithms.Any(x => x.Id == algorithm.Id))
                        continue;
                    M_Logger.Info("Storing hashrate in DB");
                    m_Storage.StoreAlgorithmData(algorithm.Id, algorithm.Name, hashRate, result.PowerUsage);
                    testedAlgorithms.Add(algorithm);
                }
                catch (Exception ex)
                {
                    M_Logger.Error(ex, "FAIL: Test failed with exception");
                    result.IsSuccess = false;
                    results.Add(result);
                }
            }

            M_Logger.Info("Test results: "
                          + Environment.NewLine
                          + string.Join(Environment.NewLine,
                              results.Select(x => $"{x.Symbol} [{x.Algorithm.Name}]: {(x.IsSuccess ? "OK" : "Fail")}, "
                                                  + $" hashrate {ConversionHelper.ToHashRateWithUnits(x.HashRate, x.Algorithm.KnownValue)},"
                                                  + $" power usage {x.PowerUsage:F2} W")));
        }

        private class TestResult
        {
            public string Symbol { get; set; }
            public bool IsSuccess { get; set; }
            public long HashRate { get; set; }
            public double PowerUsage { get; set; }
            public AlgorithmInfo Algorithm { get; set; }
        }
    }
}
