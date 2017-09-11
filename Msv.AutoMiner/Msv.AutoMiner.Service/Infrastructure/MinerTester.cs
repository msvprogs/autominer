using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Infrastructure.Contracts;
using Msv.AutoMiner.Service.Storage.Contracts;
using NLog;

namespace Msv.AutoMiner.Service.Infrastructure
{
    public class MinerTester
    {
        private static readonly ILogger M_Logger = LogManager.GetLogger("MinerTester");

        private readonly IMinerProcessController m_Controller;
        private readonly IVideoAdapterMonitor m_VideoAdapterMonitor;
        private readonly IMinerTesterStorage m_Storage;
        private readonly TimeSpan m_TestDuration;

        public MinerTester(
            IMinerProcessController controller,
            IVideoAdapterMonitor videoAdapterMonitor,
            IMinerTesterStorage storage,
            TimeSpan testDuration)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));
            if (videoAdapterMonitor == null)
                throw new ArgumentNullException(nameof(videoAdapterMonitor));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            m_Controller = controller;
            m_VideoAdapterMonitor = videoAdapterMonitor;
            m_Storage = storage;
            m_TestDuration = testDuration;
        }

        public void Test(bool benchmarkMode)
        {
            M_Logger.Info("Running miner tests (only for active coins with pools)...");

            var coinGroups = m_Storage.GetCoins()
                .Where(x => x.Activity == ActivityState.Active && x.PoolId != null)
                .GroupBy(x => x.PoolId)
                .Select(x => new
                {
                    x.First().Algorithm,
                    Coins = x.ToArray(),
                    x.First().Pool.Miner
                });
            if (benchmarkMode)
                coinGroups = coinGroups
                    .GroupBy(x => x.Algorithm)
                    .Select(x => new
                    {
                        Algorithm = x.Key,
                        Coins = x.SelectMany(y => y.Coins).Take(1).ToArray(),
                        x.First().Miner
                    });
            var testedAlgorithms = new List<CoinAlgorithm>();
            var results = new List<TestResult>();
            foreach (var coinGroup in coinGroups)
            {
                M_Logger.Info(
                    $"Testing {string.Join(", ", coinGroup.Coins.Select(x => x.Name))} [{coinGroup.Algorithm}]...");
                var result = new TestResult
                {
                    Symbol = string.Join("+", coinGroup.Coins.Select(x => x.CurrencySymbol)),
                    Algorithm = coinGroup.Algorithm
                };
                try
                {
                    m_Controller.RunNew(coinGroup.Coins, coinGroup.Miner);
                    M_Logger.Info($"Waiting {m_TestDuration.TotalMinutes:F2} minutes...");
                    var powerUsages = new List<decimal>();
                    using (Observable.Interval(TimeSpan.FromSeconds(10))
                        .Select(x => m_VideoAdapterMonitor.GetCurrentState())
                        .Where(x => x != null && x.AdapterStates?.Length > 0)
                        .Subscribe(x => powerUsages.Add(x.AdapterStates.Sum(y => y.PowerUsage))))
                    {
                        Thread.Sleep(m_TestDuration);
                    }
                    var hashRate = m_Controller.CurrentCoins.First().CurrentHashRate;
                    m_Controller.Stop();

                    if (hashRate == 0)
                    {
                        M_Logger.Error("FAIL: Something is wrong, because hashrate is zero");
                        result.IsSuccess = false;
                        results.Add(result);
                        continue;
                    }
                    M_Logger.Info(
                        $"SUCCESS: Current hashrate of {coinGroup.Algorithm} is {ConversionHelper.ToHashRateWithUnits(hashRate, coinGroup.Algorithm)}");
                    result.IsSuccess = true;
                    result.HashRate = hashRate;
                    result.PowerUsage = Math.Round((double) powerUsages.DefaultIfEmpty().Average(), 2);
                    results.Add(result);
                    if (testedAlgorithms.Contains(coinGroup.Algorithm))
                        continue;
                    M_Logger.Info("Storing hashrate in DB");
                    m_Storage.UpdateAlgorithmHashRate(coinGroup.Algorithm, hashRate, result.PowerUsage);
                    testedAlgorithms.Add(coinGroup.Algorithm);
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
                              results.Select(x => $"{x.Symbol} [{x.Algorithm}]: {(x.IsSuccess ? "OK" : "Fail")}, "
                                                  + $" hashrate {ConversionHelper.ToHashRateWithUnits(x.HashRate, x.Algorithm)},"
                                                  + $" power usage {x.PowerUsage:F2} W")));
        }

        private class TestResult
        {
            public string Symbol { get; set; }
            public bool IsSuccess { get; set; }
            public long HashRate { get; set; }
            public double PowerUsage { get; set; }
            public CoinAlgorithm Algorithm { get; set; }
        }
    }
}
