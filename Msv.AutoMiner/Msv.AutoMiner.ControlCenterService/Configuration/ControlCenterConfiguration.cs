using Msv.AutoMiner.Common.Configuration;

namespace Msv.AutoMiner.ControlCenterService.Configuration
{
    public class ControlCenterConfiguration : BaseServiceConfiguration
    {
        public string RootCertificateFileName { get; set; }
        public NotificationsElement Notifications { get; set; }
        public RigStatusLimitsElement NormalRigStateCriteria { get; set; }
        public ControlCenterServicesElement Services { get; set; }
        public ControlCenterEndpointsElement Endpoints { get; set; }
    }
}
