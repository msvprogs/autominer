using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Common.ServiceContracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.Data.Logic.Contracts;
using Msv.AutoMiner.FrontEnd.Configuration;
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
                    // To distinguish authentication between multiple instances of the frontend
                    // (for example, debug and release versions)
                    x.Cookie.Name = $"MsvAutoMinerAuth_{(uint)GetType().GetHashCode():X}";
                });

            var requireAuthPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            services
                .AddMvc(x =>
                {
                    x.Filters.Add(new AuthorizeFilter(requireAuthPolicy));
					x.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                    x.ModelBinderProviders.Insert(0, new TrimmingModelBinderProvider());
                })
                .ConfigureApplicationPartManager(x =>
                {
                    x.FeatureProviders.OfType<ViewsFeatureProvider>()
                        .ToArray()
                        .ForEach(y => x.FeatureProviders.Remove(y));
                    x.FeatureProviders.Add(new CorrectViewsFeatureProvider());
                });

            services.AddDistributedMemoryCache();
            services.AddSession(x => x.IdleTimeout = TimeSpan.MaxValue);

            var config = Configuration.Get<FrontEndConfiguration>();
            services.AddSingleton(config);
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
            services.AddSingleton<IStoredSettings, StoredSettings>();
            services.AddSingleton<IUploadedFileStorage>(
                new PhysicalUploadedFileStorage(config.FileStorage.Miners));
            services.AddSingleton<IControlCenterService>(x => new ControlCenterServiceClient(
                new AsyncRestClient(new Uri(config.Services.ControlCenter.Url))));
            services.AddSingleton<ICoinInfoService>(x => new CoinInfoServiceClient(
                new AsyncRestClient(new Uri(config.Services.CoinInfo.Url)),
                config.Services.CoinInfo.ApiKey));
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
