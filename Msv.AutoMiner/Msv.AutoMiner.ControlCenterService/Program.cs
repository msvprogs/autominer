using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Common.Log;
using Msv.AutoMiner.Common.Security;
using Msv.AutoMiner.ControlCenterService.Configuration;
using Msv.AutoMiner.ControlCenterService.External;
using Msv.AutoMiner.ControlCenterService.Logic.CommandInterfaces;
using Msv.AutoMiner.ControlCenterService.Logic.Monitors;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.ControlCenterService.Storage;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data.Logic;
using Msv.HttpTools;
using NLog;
using NLog.Targets;
using Telegram.Bot;
using ILogger = NLog.ILogger;
// ReSharper disable AccessToDisposedClosure

namespace Msv.AutoMiner.ControlCenterService
{
    public class Program
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            UnhandledExceptionHandler.RegisterLogger(M_Logger);

            Target.Register<MemoryBufferTarget>("MemoryBuffer");

            var certificateStorage = new X509CertificateStorage(
                StoreLocation.CurrentUser, new X509Certificate2(File.ReadAllBytes("rootCa.cer")));
            certificateStorage.InstallRootCertificateIfNotExist();

            var host = BuildWebHost(args);
            using (var scope = host.Services.CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<ControlCenterConfiguration>();
                using (new PoolInfoMonitor(
                    new PoolInfoProviderFactory(
                        new LoggedWebClient(),
                        new ProxiedLoggedWebClient(
                            new RoundRobinList<ProxyInfo>(ProxyList.LoadFromFile("proxies.txt")))),
                    scope.ServiceProvider.GetRequiredService<IPoolInfoMonitorStorage>()))
                using (new WalletInfoMonitor(
                    new WalletInfoProviderFactory(
                        new LoggedWebClient(),
                        () => scope.ServiceProvider.GetRequiredService<IWalletInfoProviderFactoryStorage>()),
                    scope.ServiceProvider.GetRequiredService<IWalletInfoMonitorStorage>()))
                using (new PoolAvailabilityMonitor(
                    new PoolAvailabilityChecker(new LoggedWebClient()),
                    scope.ServiceProvider.GetRequiredService<IPoolAvailabilityMonitorStorage>()))
                using (config.Notifications.Telegram.Enabled
                    ? new TelegramCommandInterface(
                        new TelegramBotClient(config.Notifications.Telegram.Token),
                        new TelegramCommandInterfaceStorage(scope.ServiceProvider
                            .GetRequiredService<IAutoMinerDbContextFactory>()),
                        new PoolInfoProvider(scope.ServiceProvider.GetRequiredService<IAutoMinerDbContextFactory>()),
                        new RigHeartbeatProvider(scope.ServiceProvider
                            .GetRequiredService<IAutoMinerDbContextFactory>()),
                        config.Notifications.Telegram.Subscribers)
                    : null)
                {
                    host.Run();
                }
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(x =>
                {
                    var config = (ControlCenterConfiguration)x.ApplicationServices.GetService(
                        typeof(ControlCenterConfiguration));
                    var http = config.Endpoints.Http;
                    if (http != null && http.Enabled)
                        x.Listen(IPAddress.Any, http.Port);

                    var httpsInternal = config.Endpoints.HttpsInternal;
                    if (httpsInternal != null && httpsInternal.Enabled)
                        x.Listen(IPAddress.Any, httpsInternal.Port, y => y.UseHttps(new HttpsConnectionAdapterOptions
                            {
                                ClientCertificateMode = ClientCertificateMode.AllowCertificate,
                                CheckCertificateRevocation = false,
                                ClientCertificateValidation = delegate { return true; },
                                ServerCertificate = new X509Certificate2(
                                    httpsInternal.Certificate.File, 
                                    httpsInternal.Certificate.Password)
                            }));

                    var httpsExternal = config.Endpoints.HttpsExternal;
                    if (httpsExternal != null && httpsExternal.Enabled)
                        x.Listen(IPAddress.Any, httpsExternal.Port, y => y.UseHttps(new HttpsConnectionAdapterOptions
                        {
                            ClientCertificateMode = ClientCertificateMode.AllowCertificate,
                            CheckCertificateRevocation = false,
                            ClientCertificateValidation = delegate { return true; },
                            ServerCertificate = new X509Certificate2(
                                httpsExternal.Certificate.File, 
                                httpsExternal.Certificate.Password)
                        }));
                })
                .UseStartup<Startup>()
                .Build();
    }
}
