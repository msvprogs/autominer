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

            services.AddSingleton<IAutoMinerDbContextFactory>(x => new AutoMinerDbContextFactory(connectionString));

            services.AddAuthentication(x =>
                {
                    x.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    x.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
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

            services.AddTransient<IStoredFiatValueProvider, StoredFiatValueProvider>();
            services.AddTransient<ICoinValueProvider, CoinValueProvider>();
            services.AddTransient<ICoinNetworkInfoProvider, CoinNetworkInfoProvider>();
            services.AddTransient<IRigHeartbeatProvider, RigHeartbeatProvider>();
            services.AddTransient<IPoolInfoProvider, PoolInfoProvider>();
            services.AddTransient<IWalletBalanceProvider, WalletBalanceProvider>();

            services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();
            services.AddSingleton<IProfitabilityCalculator, ProfitabilityCalculator>();
            services.AddSingleton<IImageProcessor, ImageProcessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IWalletAddressValidatorFactory, WalletAddressValidatorFactory>();
            services.AddSingleton<ICoinInfoService>(x => new CoinInfoServiceClient(
                new AsyncRestClient(new Uri(Configuration["Services:CoinInfo:Url"])),
                Configuration["Services:CoinInfo:ApiKey"]));
            services.AddSingleton<IControlCenterService>(x => new ControlCenterServiceClient(
                new AsyncRestClient(new Uri(Configuration["Services:ControlCenter:Url"]))));
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
