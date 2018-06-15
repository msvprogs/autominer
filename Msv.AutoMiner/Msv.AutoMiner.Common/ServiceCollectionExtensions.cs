using System;
using System.Linq;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.DependencyInjection;

namespace Msv.AutoMiner.Common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RemoveDependencyTracking(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null) 
                throw new ArgumentNullException(nameof(serviceCollection));
  
            //Disable dependency tracking telemetry HTTP headers
            var module = serviceCollection.FirstOrDefault(
                t => t.ImplementationFactory?.GetType() == typeof(Func<IServiceProvider, DependencyTrackingTelemetryModule>));
            if (module != null)
                serviceCollection.Remove(module);

            return serviceCollection;
        }
    }
}
