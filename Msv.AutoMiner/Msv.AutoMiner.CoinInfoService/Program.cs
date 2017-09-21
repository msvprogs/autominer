using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Msv.AutoMiner.CoinInfoService.External;
using Msv.AutoMiner.CoinInfoService.Logic.Monitors;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.CoinInfoService.Storage;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Data;
using NLog;
// ReSharper disable AccessToDisposedClosure

namespace Msv.AutoMiner.CoinInfoService
{
    public class Program
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            UnhandledExceptionHandler.RegisterLogger(M_Logger);

            var host = BuildWebHost(args);
            using (var scope = host.Services.CreateScope())
            {
                DbInitializer.InitializeIfNotExist(scope.ServiceProvider.GetRequiredService<AutoMinerDbContext>());

                using (new FiatValueMonitor(
                    new FiatValueProviderFactory(new LoggedWebClient()),
                    () => scope.ServiceProvider.GetRequiredService<IFiatValueMonitorStorage>()))
                using (new MarketInfoMonitor(
                    new MarketInfoProviderFactory(new LoggedWebClient()),
                    () => scope.ServiceProvider.GetRequiredService<IMarketInfoMonitorStorage>()))
                using (new NetworkInfoMonitor(
                    new NetworkInfoProviderFactory(
                        new LoggedWebClient(),
                        new DDoSTriggerPreventingWebClient(false),
                        new DDoSTriggerPreventingWebClient(true)),
                    () => scope.ServiceProvider.GetRequiredService<INetworkInfoMonitorStorage>()))
                {
                    host.Run();
                }
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
