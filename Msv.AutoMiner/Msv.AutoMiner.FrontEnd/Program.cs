using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.Data.Logic.Contracts;
using Msv.AutoMiner.FrontEnd.Configuration;
using NLog.Web;

namespace Msv.AutoMiner.FrontEnd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);

            using (var scope = host.Services.CreateScope())
                new CrossProcessDbMigrationApplier(
                        scope.ServiceProvider.GetRequiredService<IAutoMinerDbContextFactory>())
                    .ApplyIfAny();

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(x =>
                {
                    var config = (FrontEndConfiguration)x.ApplicationServices.GetService(typeof(FrontEndConfiguration));
                    var http = config.Endpoints.Http;
                    if (http != null && http.Enabled)
                        x.Listen(IPAddress.Any, http.Port);
                    var https = config.Endpoints.Https;
                    if (https != null && https.Enabled)
                        x.Listen(IPAddress.Any, https.Port, y => y.UseHttps(https.Certificate.File, https.Certificate.Password));
                })
                .UseStartup<Startup>()
                .UseNLog()
                .Build();
    }
}
