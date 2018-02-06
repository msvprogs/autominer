using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Msv.AutoMiner.CoinInfoService.Configuration;
using Msv.AutoMiner.CoinInfoService.External;
using Msv.AutoMiner.CoinInfoService.Logic.Monitors;
using Msv.AutoMiner.CoinInfoService.Logic.Profitability;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.Log;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.NetworkInfo;
using Msv.HttpTools;
using NLog;
using NLog.Targets;

// ReSharper disable AccessToDisposedClosure

namespace Msv.AutoMiner.CoinInfoService
{
    public class Program
    {
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
                using (new FiatValueMonitor(
                    new FiatValueProviderFactory(new LoggedWebClient()),
                    scope.ServiceProvider.GetRequiredService<IFiatValueMonitorStorage>()))
                using (new MarketInfoMonitor(
                    new MarketInfoProviderFactory(new LoggedWebClient()),
                    scope.ServiceProvider.GetRequiredService<IMarketInfoMonitorStorage>()))
                using (new MasternodeInfoMonitor(
                    new MasternodeInfoProviderFactory(new LoggedWebClient()),
                    scope.ServiceProvider.GetRequiredService<IMasternodeInfoStorage>()))
                using (new NetworkInfoMonitor(
                    new JsBlockRewardCalculator(),
                    scope.ServiceProvider.GetRequiredService<ICoinNetworkInfoProvider>(),
                    new NetworkInfoProviderFactory(
                        new LoggedWebClient(),
                        new ProxiedLoggedWebClient(new RoundRobinList<ProxyInfo>(ProxyList.LoadFromFile("proxies.txt")))),
                    scope.ServiceProvider.GetRequiredService<IMasternodeInfoStorage>(),
                    scope.ServiceProvider.GetRequiredService<INetworkInfoMonitorStorage>()))
                {
                    host.Run();
                }
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(x =>
                {
                    var config = (CoinInfoConfiguration) x.ApplicationServices.GetService(
                        typeof(CoinInfoConfiguration));
                    if (config.Endpoints.Http != null && config.Endpoints.Http.Enabled)
                        x.Listen(IPAddress.Any, config.Endpoints.Http.Port);
                })
                .UseStartup<Startup>()
                .Build();
    }
}
