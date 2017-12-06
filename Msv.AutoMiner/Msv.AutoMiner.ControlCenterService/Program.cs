using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.Log;
using Msv.AutoMiner.Common.Security;
using Msv.AutoMiner.ControlCenterService.External;
using Msv.AutoMiner.ControlCenterService.Logic.CommandInterfaces;
using Msv.AutoMiner.ControlCenterService.Logic.Monitors;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.ControlCenterService.Security;
using Msv.AutoMiner.ControlCenterService.Storage;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
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
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var connectionString = config.GetConnectionString("AutoMinerDb");
                using (new PoolInfoMonitor(
                    new PoolInfoProviderFactory(new LoggedWebClient()),
                    () => scope.ServiceProvider.GetRequiredService<IPoolInfoMonitorStorage>()))
                using (new WalletInfoMonitor(
                    new WalletInfoProviderFactory(
                        new LoggedWebClient(),
                        () => scope.ServiceProvider.GetRequiredService<IWalletInfoProviderFactoryStorage>()),
                    () => scope.ServiceProvider.GetRequiredService<IWalletInfoMonitorStorage>()))
                using (new TelegramCommandInterface(
                    new TelegramBotClient(config.GetValue<string>("Notifications:Telegram:Token")),
                    new TelegramCommandInterfaceStorage(connectionString),
                    config.GetSection("Notifications:Telegram:Subscribers")
                        .GetChildren()
                        .Select(y => y.Value)
                        .ToArray()))
                {
                    host.Run();
                }
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var hostingEnvironment = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile(string.Format("appsettings.{0}.json", hostingEnvironment.EnvironmentName), true,
                            true);
                    if (hostingEnvironment.IsDevelopment())
                    {
                        var assembly = Assembly.Load(new AssemblyName(hostingEnvironment.ApplicationName));
                        if (assembly != null)
                            config.AddUserSecrets(assembly, true);
                    }
                    config.AddEnvironmentVariables();
                    if (args == null)
                        return;
                    config.AddCommandLine(args);
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .UseDefaultServiceProvider((context, options) =>
                    options.ValidateScopes = context.HostingEnvironment.IsDevelopment())
                .UseKestrel(x =>
                {
                    SiteCertificates.PortCertificates
                        .ForEach(z => x.Listen(IPAddress.Any, z.Key, y =>
                        {
                            y.UseHttps(new HttpsConnectionAdapterOptions
                            {
                                ClientCertificateMode = ClientCertificateMode.AllowCertificate,
                                CheckCertificateRevocation = false,
                                ClientCertificateValidation = delegate { return true; },
                                ServerCertificate = z.Value
                            });
                        }));
                    //For internal method invocations
                    x.Listen(IPAddress.Any, 6285);
                })
                .UseStartup<Startup>()
                .Build();
    }
}
