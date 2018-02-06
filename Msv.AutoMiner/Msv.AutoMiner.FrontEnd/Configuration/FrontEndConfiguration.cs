using Msv.AutoMiner.Common.Configuration;

namespace Msv.AutoMiner.FrontEnd.Configuration
{
    public class FrontEndConfiguration : BaseServiceConfiguration
    {
        public FrontEndServicesElement Services { get; set; }
        public FrontEndEndpointsElement Endpoints { get; set; }
    }
}
