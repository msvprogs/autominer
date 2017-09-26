using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Msv.AutoMiner.CoinInfoService.Logic.Profitability;
using Msv.AutoMiner.CoinInfoService.Logic.Storage;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.CoinInfoService.Storage;
using Msv.AutoMiner.Data;

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

            services.AddDbContext<AutoMinerDbContext>(
                x => x.UseMySql(Configuration.GetConnectionString("AutoMinerDb"), y => y.CommandTimeout(30)),
                ServiceLifetime.Transient);

            services.AddMvc();

            services.AddSingleton<IProfitabilityCalculator, ProfitabilityCalculator>();
            services.AddTransient<ICoinInfoControllerStorage, CoinInfoControllerStorage>();
            services.AddTransient<IValidateApiKeyFilterStorage, ValidateApiKeyFilterStorage>();

            services.AddTransient<IFiatValueMonitorStorage, FiatValueMonitorStorage>();
            services.AddTransient<IMarketInfoMonitorStorage, MarketInfoMonitorStorage>();
            services.AddTransient<INetworkInfoMonitorStorage, NetworkInfoMonitorStorage>();
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
