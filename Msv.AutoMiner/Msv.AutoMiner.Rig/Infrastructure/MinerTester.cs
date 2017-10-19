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

        public void Test(bool benchmarkMode, string[] algorithmNames)
        {
            M_Logger.Info("Running miner tests...");
            if (algorithmNames != null)
                M_Logger.Info("Selected algorithms: " + string.Join(", ", algorithmNames));

            var algorithmSettings = m_Storage.GetMinerAlgorithmSettings()
                .Where(x => algorithmNames == null 
                    || algorithmNames.Contains(x.Algorithm.AlgorithmName, StringComparer.InvariantCultureIgnoreCase))
                .ToDictionary(x => Guid.Parse(x.AlgorithmId));
            var coins = m_ControlCenterService.GetMiningWork(
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
                .Select(x => new
                {
                    Algorithm = x.CoinAlgorithmId,
                    Coin = x,
                    MinerSetting = algorithmSettings.TryGetValue(x.CoinAlgorithmId)
                })
                .Where(x => x.MinerSetting != null
                            && (x.Coin.Pools.Any() || x.MinerSetting.Miner.BenchmarkArgument != null))
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

            var testedAlgorithms = new List<AlgorithmData>();
            var results = new List<TestResult>();
            foreach (var coinGroup in coins)
            {
                var algorithm = coinGroup.MinerSetting.Algorithm;
                M_Logger.Info(
                    $"Testing {coinGroup.Coin.CoinName} [{algorithm.AlgorithmName}]...");
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
                        BenchmarkMode = true,
                        //PoolData = coinGroup.Coin.Pools.FirstOrDefault()
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

                    if (hashRate == 0)
                    {
                        M_Logger.Error("FAIL: Something is wrong, because hashrate is zero");
                        result.IsSuccess = false;
                        results.Add(result);
                        continue;
                    }
                    M_Logger.Info(
                        $"SUCCESS: Current hashrate of {algorithm.AlgorithmName} is {ConversionHelper.ToHashRateWithUnits(hashRate, algorithm.KnownValue)}");
                    result.IsSuccess = true;
                    result.HashRate = hashRate;
                    result.PowerUsage = Math.Round((double) powerUsages.DefaultIfEmpty().Average(), 2);
                    results.Add(result);
                    if (testedAlgorithms.Any(x => x.AlgorithmId == algorithm.AlgorithmId))
                        continue;
                    M_Logger.Info("Storing hashrate in DB");
                    m_Storage.StoreAlgorithmData(Guid.Parse(algorithm.AlgorithmId), algorithm.AlgorithmName, hashRate, result.PowerUsage);
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
                              results.Select(x => $"{x.Symbol} [{x.Algorithm.AlgorithmName}]: {(x.IsSuccess ? "OK" : "Fail")}, "
                                                  + $" hashrate {ConversionHelper.ToHashRateWithUnits(x.HashRate, x.Algorithm.KnownValue)},"
                                                  + $" power usage {x.PowerUsage:F2} W")));
        }

        private class TestResult
        {
            public string Symbol { get; set; }
            public bool IsSuccess { get; set; }
            public long HashRate { get; set; }
            public double PowerUsage { get; set; }
            public AlgorithmData Algorithm { get; set; }
        }
    }
}
