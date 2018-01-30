using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Msv.AutoMiner.CoinInfoService.External;
using Msv.AutoMiner.CoinInfoService.Logic.Monitors;
using Msv.AutoMiner.CoinInfoService.Logic.Profitability;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.CoinInfoService.Storage;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.Log;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.NetworkInfo;
using Msv.BrowserCheckBypassing;
using Msv.HttpTools;
using Msv.HttpTools.Contracts;
using NLog;
using NLog.Targets;

// ReSharper disable AccessToDisposedClosure

namespace Msv.AutoMiner.CoinInfoService
{
    public class Program
    {
        private const int WebClientPoolSize = 22;
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            Target.Register<MemoryBufferTarget>("MemoryBuffer");

            UnhandledExceptionHandler.RegisterLogger(M_Logger);

            //to bypass certificates' CA validation (particularly for Linux)
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var host = BuildWebHost(args);
            using (var scope = host.Services.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<IAutoMinerDbContextFactory>().Create())
                    DbInitializer.InitializeIfNotExist(context);

                var proxyRoundRobin = new RoundRobinList<ProxyInfo>(ProxyList.LoadFromFile("proxies.txt"));
                using (var proxiedWebClientPool = new BaseWebClientPool<IProxiedBaseWebClient>(
                    () => new ProxiedWebClient(proxyRoundRobin), WebClientPoolSize))
                using (var webClientPool = new BaseWebClientPool<IBaseWebClient>(
                    CreateBaseWebClient, WebClientPoolSize))
                using (new FiatValueMonitor(
                    new FiatValueProviderFactory(new LoggedWebClient<IBaseWebClient>(webClientPool)),
                    scope.ServiceProvider.GetRequiredService<IFiatValueMonitorStorage>()))
                using (new MarketInfoMonitor(
                    new MarketInfoProviderFactory(new LoggedWebClient<IBaseWebClient>(webClientPool)),
                    scope.ServiceProvider.GetRequiredService<IMarketInfoMonitorStorage>()))
                using (new MasternodeInfoMonitor(
                    new MasternodeInfoProviderFactory(new LoggedWebClient<IBaseWebClient>(webClientPool)),
                    scope.ServiceProvider.GetRequiredService<IMasternodeInfoStorage>()))
                using (new NetworkInfoMonitor(
                    new JsBlockRewardCalculator(),
                    scope.ServiceProvider.GetRequiredService<ICoinNetworkInfoProvider>(),
                    new NetworkInfoProviderFactory(
                        new LoggedWebClient<IBaseWebClient>(webClientPool),
                        new ProxiedLoggedWebClient(proxiedWebClientPool)),
                    scope.ServiceProvider.GetRequiredService<IMasternodeInfoStorage>(),
                    scope.ServiceProvider.GetRequiredService<INetworkInfoMonitorStorage>()))
                {
                    host.Run();
                }
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

        private static IBaseWebClient CreateBaseWebClient()
            => new BrowserCheckBypassingWebClient(
                new CorrectWebClient(),
                new BrowserCheckBypasserFactory(new SolverWebClient(), MemoryClearanceCookieStorage.Instance),
                MemoryClearanceCookieStorage.Instance);
    }
}
