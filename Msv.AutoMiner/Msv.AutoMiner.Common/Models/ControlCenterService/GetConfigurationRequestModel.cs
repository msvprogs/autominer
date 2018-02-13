using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Common.Models.ControlCenterService
{
    public class GetConfigurationRequestModel : LicensedRequestBase
    {
        public PlatformType Platform { get; set; }
    }
}
