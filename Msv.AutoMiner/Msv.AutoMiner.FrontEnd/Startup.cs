using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;

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

            services.AddMvc();
            services.AddTransient<IStoredFiatValueProvider, StoredFiatValueProvider>();
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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
