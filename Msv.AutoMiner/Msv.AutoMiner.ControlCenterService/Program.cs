using System;
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
using Msv.AutoMiner.Data;
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

        private static readonly Uri[] M_HttpProxies =
        {
            new Uri("http://103.228.117.244:8080"),
            new Uri("http://37.59.62.38:8080"),
            new Uri("http://152.231.29.210:8080"),
            new Uri("http://147.135.210.114:54566"),
            new Uri("http://107.167.185.192:80"),
            new Uri("http://210.101.131.231:8080"),
            new Uri("http://186.160.80.18:3128"),
            new Uri("http://162.243.140.150:80"),
            new Uri("http://50.203.239.22:80"),
            new Uri("http://198.199.77.204:3128"),
            new Uri("http://109.121.163.56:53281"),
            new Uri("http://88.99.151.121:3128"),
            new Uri("http://77.89.249.202:8080"),
            new Uri("http://212.237.10.45:8080"),
            new Uri("http://202.157.182.141:3128"),
            new Uri("http://186.193.18.10:3128"),
            new Uri("http://167.114.47.231:3128"),
            new Uri("http://77.73.54.126:53281"),
            new Uri("http://94.230.243.6:8080"),
            new Uri("http://128.199.240.93:8080"),
            new Uri("http://51.15.83.8:3128"),
            new Uri("http://187.95.11.239:3128"),
            new Uri("http://45.55.27.246:80"),
            new Uri("http://80.211.232.151:8080"),
            new Uri("http://162.243.140.150:80"),
            new Uri("http://178.238.227.29:3128"),
            new Uri("http://80.211.165.76:8080"),
            new Uri("http://211.58.248.163:3128"),
            new Uri("http://202.59.138.138:8080"),
            new Uri("http://192.240.150.133:8080"),
            new Uri("http://85.29.136.212:8080"),
            new Uri("http://114.4.39.54:8080"),
            new Uri("http://199.195.253.124:3128"),
            new Uri("http://112.216.16.250:3128"),
            new Uri("http://162.243.140.150:8080"),
            new Uri("http://165.227.124.187:3128"),
            new Uri("http://37.17.177.197:3128"),
            new Uri("http://198.98.61.187:3128"),
            new Uri("http://160.202.41.202:8080"),
            new Uri("http://177.43.234.50:3128"),
            new Uri("http://163.172.211.176:3128"),
            new Uri("http://51.15.64.186:3128"),
            new Uri("http://109.87.23.100:8080"),
            new Uri("http://94.177.217.137:80"),
            new Uri("http://88.99.151.121:3128"),
            new Uri("http://139.59.2.223:8888"),
            new Uri("http://112.196.94.189:3128"),
            new Uri("http://94.177.217.137:8080"),
            new Uri("http://188.165.194.110:8888"),
            new Uri("http://162.243.140.150:8000"),
            new Uri("http://94.181.34.64:80"),
            new Uri("http://202.157.182.141:3128"),
            new Uri("http://176.10.255.176:8080"),
            new Uri("http://158.255.31.93:53281"),
            new Uri("http://178.161.141.154:3128"),
            new Uri("http://139.59.108.72:3128"),
            new Uri("http://128.199.189.94:8080"),
            new Uri("http://212.9.239.166:3128"),
            new Uri("http://109.185.149.65:8080"),
            new Uri("http://149.202.30.89:3128"),
            new Uri("http://178.62.117.231:3128"),
            new Uri("http://125.25.54.80:8080"),
            new Uri("http://103.39.49.10:3128"),
            new Uri("http://139.162.113.163:3128"),
            new Uri("http://91.134.199.222:25565"),
            new Uri("http://200.108.138.118:3128"),
            new Uri("http://195.80.140.212:8081"),
            new Uri("http://45.63.121.69:80"),
            new Uri("http://149.202.243.52:3128"),
            new Uri("http://62.1.84.229:8080"),
            new Uri("http://66.102.228.123:8080"),
            new Uri("http://192.241.134.233:3128"),
            new Uri("http://201.184.139.243:3128"),
            new Uri("http://45.76.189.133:3128"),
            new Uri("http://192.99.55.120:3128"),
            new Uri("http://217.182.76.229:8888"),
            new Uri("http://37.233.82.58:3130"),
            new Uri("http://190.0.22.12:3128"),
            new Uri("http://200.54.103.76:8080"),
            new Uri("http://13.74.145.66:3128"),
            new Uri("http://45.32.211.205:3128"),
            new Uri("http://190.1.137.102:3128"),
        };

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
                    new PoolInfoProviderFactory(
                        new LoggedWebClient(),
                        new ProxiedLoggedWebClient(
                            new RoundRobinList<ProxyInfo>(M_HttpProxies.Select(x => new ProxyInfo(x))))),
                    () => scope.ServiceProvider.GetRequiredService<IPoolInfoMonitorStorage>()))
                using (new WalletInfoMonitor(
                    new WalletInfoProviderFactory(
                        new LoggedWebClient(),
                        () => scope.ServiceProvider.GetRequiredService<IWalletInfoProviderFactoryStorage>()),
                    () => scope.ServiceProvider.GetRequiredService<IWalletInfoMonitorStorage>()))
                using (new TelegramCommandInterface(
                    new TelegramBotClient(config.GetValue<string>("Notifications:Telegram:Token")),
                    new TelegramCommandInterfaceStorage(connectionString),
                    new PoolInfoProvider(scope.ServiceProvider.GetRequiredService<AutoMinerDbContext>()),
                    new RigHeartbeatProvider(scope.ServiceProvider.GetRequiredService<AutoMinerDbContext>()), 
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
