using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.Security;
using Msv.AutoMiner.ControlCenterService.External;
using Msv.AutoMiner.ControlCenterService.Logic.Monitors;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using NLog;
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

            var certificateStorage = new X509CertificateStorage(
                StoreLocation.CurrentUser, new X509Certificate2(File.ReadAllBytes("rootCa.cer")));
            certificateStorage.InstallRootCertificateIfNotExist();

            var host = BuildWebHost(args);
            using (var scope = host.Services.CreateScope())
            {
                using (new PoolInfoMonitor(
                    new PoolInfoProviderFactory(new LoggedWebClient()),
                    () => scope.ServiceProvider.GetRequiredService<IPoolInfoMonitorStorage>()))
                using (new WalletInfoMonitor(
                    new WalletInfoProviderFactory(
                        new LoggedWebClient(),
                        () => scope.ServiceProvider.GetRequiredService<IWalletInfoProviderFactoryStorage>()),
                    () => scope.ServiceProvider.GetRequiredService<IWalletInfoMonitorStorage>()))
                {
                    host.Run();
                }
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(x =>
                {
                    x.Listen(IPAddress.Any, 6283, y =>
                    {
                        y.UseHttps(new HttpsConnectionAdapterOptions
                        {
                            ClientCertificateMode = ClientCertificateMode.AllowCertificate,
                            ClientCertificateValidation = delegate { return true; },
                            ServerCertificate = new X509Certificate2(File.ReadAllBytes("controlService.pfx"), "vl01fgNUNRFWttb37yst")
                        });
                    });
                })
                .UseStartup<Startup>()
                .Build();
    }
}
