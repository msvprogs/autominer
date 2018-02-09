using System;
using System.Linq;
using System.Reflection;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Common.ServiceContracts;
using Msv.AutoMiner.ControlCenterService.Configuration;
using Msv.AutoMiner.ControlCenterService.Logic.Analyzers;
using Msv.AutoMiner.ControlCenterService.Logic.Notifiers;
using Msv.AutoMiner.ControlCenterService.Logic.Storage;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.ControlCenterService.Security;
using Msv.AutoMiner.ControlCenterService.Security.Contracts;
using Msv.AutoMiner.ControlCenterService.Storage;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data.Logic;
using Telegram.Bot;

namespace Msv.AutoMiner.ControlCenterService
{
    [Obfuscation(Exclude = true)]
    public class Startup
    {
        public Startup(IConfiguration configuration) 
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFrameworkMySql();

            services.AddSingleton<IAutoMinerDbContextFactory>(
                x => new AutoMinerDbContextFactory(Configuration.GetConnectionString("AutoMinerDb")));

            services.AddMvc();

            //Disable dependency tracking telemetry HTTP headers
            var module = services.FirstOrDefault(
                t => t.ImplementationFactory?.GetType() == typeof(Func<IServiceProvider, DependencyTrackingTelemetryModule>));
            if (module != null)
                services.Remove(module);

            var config = Configuration.Get<ControlCenterConfiguration>();
            services.AddSingleton(config);
            services.AddSingleton<ICertificateServiceStorage, CertificateServiceStorage>();
            services.AddSingleton<IControlCenterControllerStorage, ControlCenterControllerStorage>();
            services.AddSingleton<IWalletInfoProviderFactoryStorage, WalletInfoProviderFactoryStorage>();
            services.AddSingleton<IPoolInfoMonitorStorage, PoolInfoMonitorStorage>();
            services.AddSingleton<IWalletInfoMonitorStorage, WalletInfoMonitorStorage>();
            services.AddSingleton<INotifierStorage, NotifierStorage>();
            services.AddSingleton<IPoolAvailabilityMonitorStorage, PoolAvailabilityMonitorStorage>();
            services.AddSingleton<IMiningWorkBuilderStorage, MiningWorkBuilderStorage>();
            services.AddSingleton<IMiningWorkBuilder, MiningWorkBuilder>();

            if (config.Notifications.Telegram.Enabled)
                services.AddSingleton<ITelegramBotClient>(
                    x => new TelegramBotClient(config.Notifications.Telegram.Token));

            services.AddSingleton(x => new HeartbeatAnalyzerParams
            {
                SamplesCount = config.NormalRigStateCriteria.SamplesCount,
                MaxInvalidSharesRate = config.NormalRigStateCriteria.MaxInvalidSharesRate,
                MaxHashrateDifference = config.NormalRigStateCriteria.MaxHashrateDifference,
                MinVideoUsage = config.NormalRigStateCriteria.MinVideoUsage,
                MaxVideoTemperature = config.NormalRigStateCriteria.MaxVideoTemperature
            });

            if (config.Notifications.Telegram.Enabled)
                services.AddSingleton<INotifier>(
                    x => new TelegramNotifier(
                        x.GetRequiredService<ITelegramBotClient>(),
                        x.GetRequiredService<INotifierStorage>(),
                        config.Notifications.Telegram.Subscribers));
            else
                services.AddSingleton<INotifier>(new DummyNotifier());

            services.AddSingleton<IHeartbeatAnalyzer, HeartbeatAnalyzer>();

            services.AddSingleton<ICertificateService, CertificateService>();
            services.AddSingleton<IUploadedFileStorage>(new PhysicalUploadedFileStorage(config.FileStorage.Miners));
            services.AddSingleton<IConfigurationHasher, Sha256ConfigurationHasher>();
            services.AddSingleton<ICoinInfoService>(x => new CoinInfoServiceClient(
                new AsyncRestClient(new Uri(config.Services.CoinInfo.Url)),
                    config.Services.CoinInfo.ApiKey));
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
