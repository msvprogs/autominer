using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Msv.AutoMiner.Rig.Commands;
using Msv.AutoMiner.Rig.Data;
using Msv.AutoMiner.Rig.Infrastructure;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;
using Msv.AutoMiner.Rig.Properties;
using Msv.AutoMiner.Rig.Remote;
using Msv.AutoMiner.Rig.Security;
using Msv.AutoMiner.Rig.Storage;
using Msv.AutoMiner.Rig.Storage.Model;
using Msv.AutoMiner.Rig.System;
using Msv.AutoMiner.Rig.System.Video.NVidia;
using NLog;

// ReSharper disable AccessToDisposedClosure

namespace Msv.AutoMiner.Rig
{
    internal class Program
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException +=
                (s, e) => M_Logger.Fatal(e.ExceptionObject as Exception, "Unhandled exception");
            TaskScheduler.UnobservedTaskException +=
                (s, e) =>
                {
                    M_Logger.Error(e.Exception, "Unobserved exception");
                    e.SetObserved();
                };

            //TODO: temporary
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var certificateStorage = new X509CertificateStorageFactory(
                StoreLocation.CurrentUser, new X509Certificate2(File.ReadAllBytes("rootCa.cer"))).Create();
            certificateStorage.InstallRootCertificateIfNotExist();

            var certificateProvider = new ClientCertificateProvider(certificateStorage, new StoredSettings());
            var controlCenterClient = new ControlCenterServiceClient(
                new WebRequestRestClient(new Uri(Settings.Default.ControlCenterServiceUrl))
                {
                    ClientCertificate = certificateProvider.GetCertificate()
                });

            AlgorithmData[] algorithmDatas;
            using (var storage = new AutoMinerRigDbContext())
                algorithmDatas = storage.AlgorithmDatas.ToArray();

            using (var childTracker = new ChildProcessTrackerFactory().Create())
            using (var controller = new MinerProcessController(
                new EnvironmentVariableCreatorFactory(Settings.Default.UnixCudaLibraryPath).Create(),
                new ProcessStopperFactory().Create(),
                childTracker,
                Settings.Default.ShareTimeout,
                algorithmDatas))
            {
                var videoStateProvider = new NVidiaVideoSystemStateProvider();
                var interpreter = new CommandInterpreter(
                    new CommandProcessor(
                        videoStateProvider,
                        controlCenterClient,
                        new ControlCenterRegistrator(
                            certificateProvider,
                            controlCenterClient),
                        new MinerTester(
                            controller,
                            videoStateProvider,
                            controlCenterClient,
                            new MinerTesterStorage(),
                            Settings.Default.TestModeMiningDuration),
                        new CommandProcessorStorage()));
                if (interpreter.Interpret(args))
                    return;

                if (certificateProvider.GetCertificate() == null)
                {
                    Console.WriteLine("This rig isn't registered! Register it at control center.");
                    return;
                }

                var started = DateTime.Now;
                var delayProvider = new PeriodicTaskDelayProvider(certificateProvider);
                using (Observable.Interval(TimeSpan.FromSeconds(10))
                    .Where(x => controller.CurrentState != null)
                    .Subscribe(x =>
                        M_Logger.Trace($"Mining {controller.CurrentState.ToString()}; uptime {DateTime.Now - started}")))
                using (new AutomaticMinerChanger(
                    controller,
                    new MiningProfitabilityTableBuilder(
                        controlCenterClient,
                        new MiningProfitabilityTableBuilderStorage(),
                        new ProfitabilityTableBuilderParams
                        {
                            ElectricityKwhCostUsd = Settings.Default.ElectricityKwhCostUsd,
                            SystemPowerUsageWatts = Settings.Default.SystemPowerUsageWatts
                        }),
                    new PoolStatusProvider(),
                    delayProvider,
                    new MinerChangingOptions
                    {
                        Interval = Settings.Default.ProfitabilityQueryInterval,
                        Dispersion = Settings.Default.ProfitabilityQueryDispersion,
                        ThresholdRatio = Settings.Default.CurrencyChangeThresholdRatio
                    }))
                using (var videoAdapterMonitor = new VideoAdapterMonitor(videoStateProvider))
                using (new HeartbeatSender(
                    new SystemStateProviderFactory().Create(), 
                    videoAdapterMonitor, 
                    controller, 
                    controlCenterClient,
                    delayProvider,
                    new HeartbeatSenderStorage(),
                    Settings.Default.SystemPowerUsageWatts))
                using (CreateWatchdogDisposable(videoAdapterMonitor))
                {
                    M_Logger.Info("Automatic miner controller started.");
                    M_Logger.Info("Press Ctrl+C to exit");
                    Console.TreatControlCAsInput = true;
                    ConsoleKeyInfo key;
                    do
                    {
                        key = Console.ReadKey(true);
                    } while (!IsCtrlCKey(key));
                    M_Logger.Debug("Exiting (press Ctrl+C again to exit immediately)...");
                    Observable.Interval(TimeSpan.FromMilliseconds(100))
                        .Where(x => Console.KeyAvailable)
                        .Select(x => Console.ReadKey(true))
                        .Where(IsCtrlCKey)
                        .Take(1)
                        .Subscribe(x =>
                        {
                            M_Logger.Warn("Exiting forcibly...");
                            Environment.Exit(0);
                        });
                }
            }
        }

        private static bool IsCtrlCKey(ConsoleKeyInfo key)
            => key.Modifiers.HasFlag(ConsoleModifiers.Control) && key.Key == ConsoleKey.C;

        private static IDisposable CreateWatchdogDisposable(IVideoAdapterMonitor videoAdapterMonitor)
        {
            if (!Settings.Default.UseWatchdog)
                return Disposable.Empty;
            var watchdog = new WatchdogFactory().Create();
            return new CompositeDisposable(
                watchdog,
                Observable.Interval(TimeSpan.FromSeconds(30))
                    .StartWith(0)
                    .Where(x => videoAdapterMonitor.IsAlive)
                    .Subscribe(x => watchdog.Feed()));
        }
    }
}