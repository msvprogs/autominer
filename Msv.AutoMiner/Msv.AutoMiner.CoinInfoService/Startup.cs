using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Msv.AutoMiner.CoinInfoService.Logic.Storage;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.CoinInfoService.Storage;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.CoinInfoService
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

            services.AddSingleton<IAutoMinerDbContextFactory>(
                x => new AutoMinerDbContextFactory(Configuration.GetConnectionString("AutoMinerDb")));

            services.AddMvc();

            services.AddSingleton<IProfitabilityCalculator, ProfitabilityCalculator>();
            services.AddSingleton<IProfitabilityTableBuilder, ProfitabilityTableBuilder>();

            services.AddSingleton<ICoinInfoControllerStorage, CoinInfoControllerStorage>();
            services.AddSingleton<IValidateApiKeyFilterStorage, ValidateApiKeyFilterStorage>();
            services.AddSingleton<ICoinNetworkInfoProvider, CoinNetworkInfoProvider>();
            services.AddSingleton<ICoinValueProvider, CoinValueProvider>();

            services.AddSingleton<IFiatValueMonitorStorage, FiatValueMonitorStorage>();
            services.AddSingleton<IMarketInfoMonitorStorage, MarketInfoMonitorStorage>();
            services.AddSingleton<INetworkInfoMonitorStorage, NetworkInfoMonitorStorage>();
            services.AddSingleton<IStoredFiatValueProvider, StoredFiatValueProvider>();
            services.AddSingleton<IMasternodeInfoStorage, MasternodeInfoMemoryStorage>();
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
