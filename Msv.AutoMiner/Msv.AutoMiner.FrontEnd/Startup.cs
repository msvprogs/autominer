using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Common.ServiceContracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.FrontEnd.Infrastructure;
using Msv.AutoMiner.FrontEnd.Providers;

namespace Msv.AutoMiner.FrontEnd
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

            services.AddMvc()
                .AddMvcOptions(x => x.ModelBinderProviders.Insert(0, new TrimmingModelBinderProvider()));
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddTransient<IStoredFiatValueProvider, StoredFiatValueProvider>();
            services.AddTransient<ICoinValueProvider, CoinValueProvider>();
            services.AddTransient<ICoinNetworkInfoProvider, CoinNetworkInfoProvider>();
            services.AddTransient<IRigHeartbeatProvider, RigHeartbeatProvider>();
            services.AddTransient<IPoolInfoProvider, PoolInfoProvider>();
            services.AddTransient<IWalletBalanceProvider, WalletBalanceProvider>();

            services.AddSingleton<IProfitabilityCalculator, ProfitabilityCalculator>();
            services.AddSingleton<IImageProcessor, ImageProcessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<ICoinInfoService>(x => new CoinInfoServiceClient(
                new AsyncRestClient(new Uri(Configuration["Services:CoinInfo:Url"])),
                Configuration["Services:CoinInfo:ApiKey"]));
            services.AddSingleton<IControlCenterService>(x => new ControlCenterServiceClient(
                new AsyncRestClient(new Uri(Configuration["Services:ControlCenter:Url"]))));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
