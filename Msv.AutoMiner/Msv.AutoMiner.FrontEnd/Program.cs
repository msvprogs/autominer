using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Msv.AutoMiner.FrontEnd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(x =>
                {
                    // x.Listen(IPAddress.Any, 4500);
                    x.Listen(IPAddress.Any, 4501, y => y.UseHttps("controlCenterEx1.pfx", "12345"));
                })
                .UseStartup<Startup>()
                .Build();
    }
}
