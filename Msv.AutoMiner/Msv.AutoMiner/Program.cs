using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Properties;
using Msv.AutoMiner.Service.External;
using Msv.AutoMiner.Service.External.Network;
using Msv.AutoMiner.Service.Infrastructure;
using Msv.AutoMiner.Service.Infrastructure.Contracts;
using Msv.AutoMiner.Service.Infrastructure.Data;
using Msv.AutoMiner.Service.Security;
using Msv.AutoMiner.Service.Storage;
using Msv.AutoMiner.Service.System;
using Msv.AutoMiner.Service.Video.NVidia;
using NLog;
using Msv.AutoMiner.Commons;
using Msv.AutoMiner.Service.Data;

// ReSharper disable AccessToDisposedClosure

namespace Msv.AutoMiner
{
    internal class Program
    {
        private static readonly ILogger M_Logger = LogManager.GetLogger("Program");

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException +=
                (s, e) => M_Logger.Fatal(e.ExceptionObject as Exception, "Unhandled exception");
            ServicePointManager.DefaultConnectionLimit = 32;

            string[] explicitCurrencies = null;
            var testMode = false;
            if (args.Any())
            {
                if (ProcessArgs(args))
                    return;
                var currencyArg = Array.IndexOf(args, "--currencies");
                if (currencyArg >= 0)
                {
                    explicitCurrencies = args[currencyArg + 1].Split(',')
                        .Select(x => x.ToUpperInvariant())
                        .ToArray();
                    M_Logger.Info("Mining only " + string.Join(", ", explicitCurrencies));
                }
                testMode = args.Contains("--test");
            }

            AlgorithmData[] algorithmDatas;
            using (var storage = new AutoMinerDbContext())
                algorithmDatas = storage.AlgorithmDatas.ToArray();

            if (testMode)
            {
                using (var childTracker = new ChildProcessTrackerFactory().Create())
                using (var controller = new MinerProcessController(
                    new EnvironmentVariableCreatorFactory(Settings.Default.UnixCudaLibraryPath).Create(),
                    new ProcessStopperFactory().Create(),
                    childTracker,
                    Settings.Default.ShareTimeout,
                    algorithmDatas))
                using (var monitor = new VideoAdapterMonitor(
                    new NVidiaVideoSystemStateProvider()))
                {
                    var tester = new MinerTester(
                        controller,
                        monitor,
                        new MinerTesterStorage(explicitCurrencies),
                        Settings.Default.TestModeMiningDuration);
                    tester.Test(args.Contains("--benchmark"));
                    M_Logger.Info("Miner tests completed");
                    return;
                }
            }

            Console.TreatControlCAsInput = true;

            var downloader = new DDoSTriggerPreventingDownloader();
            var started = DateTime.Now;
            using (var childTracker = new ChildProcessTrackerFactory().Create())
            using (var controller = new MinerProcessController(
                new EnvironmentVariableCreatorFactory(Settings.Default.UnixCudaLibraryPath).Create(),
                new ProcessStopperFactory().Create(),
                childTracker,
                Settings.Default.ShareTimeout,
                algorithmDatas))
            using (Observable.Interval(TimeSpan.FromSeconds(10))
                .Where(x => (controller.CurrentCoins?.Any()).GetValueOrDefault())
                .Subscribe(x => M_Logger.Trace($"Mining {string.Join(", ", controller.CurrentCoins.Select(y => y.ToString()))}"
                    + $"; mode: {controller.CurrentMode}; uptime {DateTime.Now - started}")))
            using (var changer = new AutomaticMinerChanger(
                controller,
                new ProfitabilityTableBuilder(
                    new CoinNetworkInfoUpdater(
                        new CoinNetworkInfoProviderFactory(),
                        downloader,
                        new CoinNetworkInfoUpdaterStorage(explicitCurrencies)),
                    new MarketValuesProvider(
                        new CoinMarketInfoProviderFactory()),
                    new CoinMarketInfoAggregator(
                        new CoinMarketInfoAggregatorStorage()),
                    new ConsolidationRouteBuilder(
                        new ConsolidationRouteBuilderStorage()), 
                    new ProfitabilityCalculator(),
                    new BitCoinInfoProvider(), 
                    new ProfitabilityTableBuilderStorage(),
                    new ProfitabilityTableBuilderParams
                    {
                        ElectricityKwhCostUsd = Settings.Default.ElectricityKwhCostUsd,
                        SystemPowerUsageWatts = Settings.Default.SystemPowerUsageWatts
                    }),
                new AutomaticMinerChangerStorage(),
                new PoolStatusProvider(
                    new PoolStatusProviderStorage()),
                new MinerChangingOptions
                {
                    Interval = Settings.Default.ProfitabilityQueryInterval,
                    Dispersion = Settings.Default.ProfitabilityQueryDispersion,
                    ThresholdRatio = Settings.Default.CurrencyChangeThresholdRatio
                }))
            using (new PoolAccountMonitor(changer, new PoolInfoProviderFactory(), downloader, new PoolAccountMonitorStorage()))
            using (new ExchangeAccountMonitor(new ExchangeTraderFactory(
                new StringEncryptor(), new ExchangeTraderFactoryStorage()), new ExchangeAccountMonitorStorage()))
            using (var monitor = new VideoAdapterMonitor(
                new NVidiaVideoSystemStateProvider()))
            using (CreateWatchdogDisposable(monitor))
            {
                M_Logger.Info("Automatic miner controller started.");
                M_Logger.Info("Type 'exit' or press Ctrl+C to exit");
                var keysList = new List<ConsoleKeyInfo>();
                var exitKeys = new[] { ConsoleKey.E, ConsoleKey.X, ConsoleKey.I, ConsoleKey.T, ConsoleKey.Enter };
                do
                {
                    if (keysList.Count > exitKeys.Length)
                        keysList.RemoveAt(0);
                    keysList.Add(Console.ReadKey(false));
                } while (!keysList[0].Modifiers.HasFlag(ConsoleModifiers.Control)
                         && keysList[0].Key != ConsoleKey.C
                         && !keysList.Select(x => x.Key).SequenceEqual(exitKeys));
                M_Logger.Debug("Exiting...");
            }
        }

        private static bool ProcessArgs(string[] args)
        {
            if (args.Contains("-h"))
            {
                DisplayHelp();
                return true;
            }
            if (args[0] == "--reg-exchange-keys" && args.Length >= 4)
            {
                ExchangeType exchange;
                if (!Enum.TryParse(args[1], true, out exchange))
                {
                    Console.WriteLine($"Unknown exchange: {args[1]}");
                    return true;
                }
                var encryptor = new StringEncryptor();
                var storage = new ExchangeKeyStorage();
                storage.StoreKey(exchange, args[2], encryptor.Encrypt(args[3]));
                Console.WriteLine("Keys were registered successfully!");
                return true;
            }
            if (args[0] == "--estimate")
            {
                EstimateProfitability(args.Skip(1).ToArray());
                return true;
            }
            return false;
        }

        private static void EstimateProfitability(string[] args)
        {
            var argsDic = args
                .Where(x => x.Contains(":"))
                .Select(x => x.Split(new[] {':'}, 2))
                .GroupBy(x => x[0], StringComparer.InvariantCultureIgnoreCase)
                .ToDictionary(x => x.Key.ToUpperInvariant(), x => x.First()[1]);
            var algorithmStr = argsDic.TryGetValue("A");
            var difficultyStr = argsDic.TryGetValue("D");
            var hashrateStr = argsDic.TryGetValue("H");
            var rewardStr = argsDic.TryGetValue("R");
            var solsPerDiffStr = argsDic.TryGetValue("SPD");
            if (algorithmStr == null && hashrateStr == null)
            {
                Console.WriteLine("You must specify either algorithm or hashrate");
                return;
            }
            if (difficultyStr == null)
            {
                Console.WriteLine("You must specify difficulty");
                return;
            }
            if (rewardStr == null)
            {
                Console.WriteLine("You must specify reward");
                return;
            }

            long hashrate;
            var algorithm = CoinAlgorithm.Unknown;
            if (hashrateStr != null)
            {
                hashrate = ParsingHelper.ParseHashRate(hashrateStr);
                if (hashrate == 0)
                {
                    Console.WriteLine("Invalid hashrate");
                    return;
                }
            }
            else
            {
                if (!Enum.TryParse(algorithmStr, true, out algorithm))
                {
                    Console.WriteLine("Unknown algorithm");
                    return;
                }

                long? storedRate;
                using (var storage = new AutoMinerDbContext())
                    storedRate = storage.AlgorithmDatas.FirstOrDefault(x => x.Algorithm == algorithm)?.SpeedInHashes;
                if (storedRate == null)
                {
                    Console.WriteLine("There is no stored hashrate for this algorithm, specify it explicitly");
                    return;
                }
                hashrate = storedRate.Value;
            }

            var calculator = new ProfitabilityCalculator();
            var result = calculator.CalculateCoinsPerDay(
                new Coin
                {
                    Algorithm = algorithm,
                    SolsPerDiff = solsPerDiffStr != null ? int.Parse(solsPerDiffStr) : (int?)null,
                    BlockReward = ParsingHelper.ParseDouble(rewardStr),
                    Difficulty = ParsingHelper.ParseDouble(difficultyStr)
                },
                hashrate);
            Console.WriteLine($"Estimated coins per day: {result:N6}");
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("--reg-exchange-keys <exchange> <api-key> <secret> - register API keys for exchange");
            Console.WriteLine("Example: --reg-exchange-keys poloniex 12873b585b 363b049630b68346");
            Console.WriteLine("--currencies <currencies> - use only specified currencies, separated by comma");
            Console.WriteLine("Example: --currencies LTC,ZEC,ZCL");
            Console.WriteLine("--test - run application in test mode (runs all miners sequentially, detects errors and measures hashrates)");
            Console.WriteLine("--test --benchmark - run application in test mode with testing only one currency per algorithm");
            Console.WriteLine();
            Console.WriteLine("--estimate <args> - estimate coins per day for specified mining parameters");
            Console.WriteLine("Parameters: A - algorithm, D - difficulty, H - hashrate, R - block reward, SPD - sols per diff (for equihash)");
            Console.WriteLine("Example for known algorithm: --estimate A:tribus D:145.222 R:10.5");
            Console.WriteLine("Example for unknown algorithm: --estimate \"H:789 Mh/s\" D:100893 R:2.4");
        }

        private static IDisposable CreateWatchdogDisposable(IVideoAdapterMonitor videoAdapterMonitor)
        {
            if (!Settings.Default.UseWatchdog)
                return Disposable.Empty;
            var watchdog = new WatchdogFactory().Create();
            return new CompositeDisposable(
                watchdog,
                Observable.Interval(TimeSpan.FromSeconds(30))
                    .StartWith(0)
                    .Where(x => videoAdapterMonitor.AreAlive)
                    .Subscribe(x => watchdog.Feed()));
        }
    }
}

