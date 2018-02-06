using Msv.AutoMiner.Common.Configuration;

namespace Msv.AutoMiner.FrontEnd.Configuration
{
    public class FrontEndEndpointsElement
    {
        public EndpointElement Http { get; set; }
        public SslEndpointElement Https { get; set; }
    }
}
