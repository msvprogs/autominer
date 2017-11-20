using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.ServiceContracts;
using Msv.AutoMiner.ControlCenterService.Logic.Analyzers;
using Msv.AutoMiner.ControlCenterService.Logic.Notifiers;
using Msv.AutoMiner.ControlCenterService.Logic.Storage;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.ControlCenterService.Security;
using Msv.AutoMiner.ControlCenterService.Security.Contracts;
using Msv.AutoMiner.ControlCenterService.Storage;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;
using Telegram.Bot;

namespace Msv.AutoMiner.ControlCenterService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFrameworkMySql();

            var connectionString = Configuration.GetConnectionString("AutoMinerDb");
            services.AddDbContext<AutoMinerDbContext>(
                x => x.UseMySql(connectionString, y => y.CommandTimeout(30)),
                ServiceLifetime.Transient);

            services.AddMvc();

            services.AddSingleton(x => Configuration);
            services.AddTransient<ICertificateServiceStorage, CertificateServiceStorage>();
            services.AddTransient<IControlCenterControllerStorage, ControlCenterControllerStorage>();
            services.AddTransient<IWalletInfoProviderFactoryStorage, WalletInfoProviderFactoryStorage>();

            services.AddSingleton<ITelegramBotClient, TelegramBotClient>(
                x => new TelegramBotClient(Configuration.GetValue<string>("Notifications:Telegram:Token")));
            services.AddSingleton<IPoolInfoMonitorStorage, PoolInfoMonitorStorage>(
                x => new PoolInfoMonitorStorage(connectionString));
            services.AddSingleton<IWalletInfoMonitorStorage, WalletInfoMonitorStorage>(
                x => new WalletInfoMonitorStorage(connectionString));
            services.AddSingleton<IRigStatusNotifierStorage, RigStatusNotifierStorage>(
                x => new RigStatusNotifierStorage(connectionString));
            services.AddSingleton(x => new HeartbeatAnalyzerParams
            {
                SamplesCount = Configuration.GetValue<int>("NormalRigStateCriteria:SamplesCount"),
                MaxInvalidSharesRate = Configuration.GetValue<int>("NormalRigStateCriteria:MaxInvalidSharesRate"),
                MaxHashrateDifference = Configuration.GetValue<int>("NormalRigStateCriteria:MaxHashrateDifference"),
                MinVideoUsage = Configuration.GetValue<int>("NormalRigStateCriteria:MinVideoUsage"),
                MaxVideoTemperature = Configuration.GetValue<int>("NormalRigStateCriteria:MaxVideoTemperature")
            });
            //@autominer_test
            services.AddSingleton<IRigStatusNotifier, TelegramRigStatusNotifier>(
                x => new TelegramRigStatusNotifier(
                    x.GetRequiredService<ITelegramBotClient>(),
                    x.GetRequiredService<IRigStatusNotifierStorage>(),
                    Configuration.GetSection("Notifications:Telegram:Subscribers")
                        .GetChildren()
                        .Select(y => y.Value)
                        .ToArray()));
            services.AddSingleton<IHeartbeatAnalyzer, HeartbeatAnalyzer>();

            services.AddTransient<ICertificateService, CertificateService>();
            services.AddSingleton<ICoinInfoService>(x => new CoinInfoServiceClient(
                new AsyncRestClient(new Uri(Configuration["Services:CoinInfo:Url"])),
                Configuration["Services:CoinInfo:ApiKey"]));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
