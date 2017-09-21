using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.ServiceContracts;
using Msv.AutoMiner.ControlCenterService.External.CoinInfoService;
using Msv.AutoMiner.ControlCenterService.Logic.Storage;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.ControlCenterService.Security;
using Msv.AutoMiner.ControlCenterService.Security.Contracts;
using Msv.AutoMiner.ControlCenterService.Storage;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;

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
            services.AddEntityFrameworkSqlite();

            services.AddDbContext<AutoMinerDbContext>(
                x => x.UseSqlite(Configuration.GetConnectionString("AutoMinerDb")),
                ServiceLifetime.Transient);

            services.AddMvc();

            services.AddTransient<ICertificateServiceStorage, CertificateServiceStorage>();
            services.AddTransient<IControlCenterControllerStorage, ControlCenterControllerStorage>();
            services.AddTransient<IWalletInfoProviderFactoryStorage, WalletInfoProviderFactoryStorage>();
            services.AddTransient<IPoolInfoMonitorStorage, PoolInfoMonitorStorage>();
            services.AddTransient<IWalletInfoMonitorStorage, WalletInfoMonitorStorage>();

            services.AddSingleton<ICertificateService>(x => new CertificateService(
                new X509Certificate2(
                    File.ReadAllBytes(Configuration["CaCertificate:File"]),
                    Configuration["CaCertificate:Password"]),
                x.GetRequiredService<ICertificateServiceStorage>()));
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
