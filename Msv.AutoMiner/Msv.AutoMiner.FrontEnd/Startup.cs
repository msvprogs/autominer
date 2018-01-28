using System;
using System.IO.Compression;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.ResponseCompression;
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
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;
using Msv.AutoMiner.FrontEnd.Providers;
using Msv.AutoMiner.NetworkInfo;

namespace Msv.AutoMiner.FrontEnd
{
    public class Startup
    {        
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFrameworkMySql();

            var connectionString = Configuration.GetConnectionString("AutoMinerDb");
            services.AddDbContext<AutoMinerDbContext>(
                x => x.UseMySql(connectionString, y => y.CommandTimeout(30)),
                ServiceLifetime.Transient);

            services.AddSingleton<IAutoMinerDbContextFactory>(x => new AutoMinerDbContextFactory(connectionString));

            services.AddAuthentication(x =>
                {
                    x.DefaultSignInScheme = 
                        x.DefaultAuthenticateScheme = 
                            x.DefaultChallengeScheme = 
                                x.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(x =>
                {
                    x.LoginPath = "/Authentication/Login";
                    x.LogoutPath = "/Authentication/Logout";
                    x.AccessDeniedPath = "/Authentication/AccessDenied";
                });

            var requireAuthPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            services.AddMvc(x => x.Filters.Add(new AuthorizeFilter(requireAuthPolicy)))
                .AddMvcOptions(x => x.ModelBinderProviders.Insert(0, new TrimmingModelBinderProvider()));

            services.AddDistributedMemoryCache();
            services.AddSession(x => x.IdleTimeout = TimeSpan.MaxValue);

            services.Configure<GzipCompressionProviderOptions>(x => x.Level = CompressionLevel.Optimal);
            services.AddResponseCompression();

            services.AddSingleton<IStoredFiatValueProvider, StoredFiatValueProvider>();
            services.AddSingleton<ICoinValueProvider, CoinValueProvider>();
            services.AddSingleton<ICoinNetworkInfoProvider, CoinNetworkInfoProvider>();
            services.AddSingleton<IRigHeartbeatProvider, RigHeartbeatProvider>();
            services.AddSingleton<IPoolInfoProvider, PoolInfoProvider>();
            services.AddTransient<IWalletBalanceProvider, WalletBalanceProvider>();

            services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();
            services.AddSingleton<IProfitabilityCalculator, ProfitabilityCalculator>();
            services.AddSingleton<IImageProcessor, ImageProcessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IWalletAddressValidatorFactory, WalletAddressValidatorFactory>();
            services.AddSingleton<INetworkInfoProviderFactory>(x => new NetworkInfoProviderFactory(
                new DummyWebClient(), new DummyWebClient()));
            services.AddSingleton<IBlockExplorerUrlProviderFactory, BlockExplorerUrlProviderFactory>();
            services.AddSingleton<ICryptoRandomGenerator, CryptoRandomGenerator>();
            services.AddSingleton<IProfitabilityTableBuilder, ProfitabilityTableBuilder>();
            services.AddSingleton<IMiningWorkBuilderStorage, MiningWorkBuilderStorage>();
            services.AddSingleton<IMiningWorkBuilder, MiningWorkBuilder>();
            services.AddSingleton<IOverallProfitabilityCalculator, OverallProfitabilityCalculator>();
            services.AddSingleton<IUploadedFileStorage>(new PhysicalUploadedFileStorage(
                Configuration["FileStorage:Miners"]));
            services.AddSingleton<IControlCenterService>(x => new ControlCenterServiceClient(
                new AsyncRestClient(new Uri(Configuration["Services:ControlCenter:Url"]))));
            services.AddSingleton<ICoinInfoService>(x => new CoinInfoServiceClient(
                new AsyncRestClient(new Uri(Configuration["Services:CoinInfo:Url"])),
                Configuration["Services:CoinInfo:ApiKey"]));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseResponseCompression();

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

            app.UseAuthentication();
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
