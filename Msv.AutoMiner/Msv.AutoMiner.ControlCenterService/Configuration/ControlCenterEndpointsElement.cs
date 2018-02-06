using Msv.AutoMiner.Common.Configuration;

namespace Msv.AutoMiner.ControlCenterService.Configuration
{
    public class ControlCenterEndpointsElement
    {
        public EndpointElement Http { get; set; }
        public SslEndpointElement HttpsInternal { get; set; }
        public SslEndpointElement HttpsExternal { get; set; }

        public SslEndpointElement EndpointFromPort(int port)
            => port == HttpsExternal?.Port
                ? HttpsExternal
                : port == HttpsInternal?.Port
                    ? HttpsInternal
                    : null;
    }
}
