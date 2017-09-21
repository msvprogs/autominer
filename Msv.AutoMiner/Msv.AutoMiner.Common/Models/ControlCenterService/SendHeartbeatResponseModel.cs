using Msv.AutoMiner.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msv.AutoMiner.Common.Models.ControlCenterService
{
    public class SendHeartbeatResponseModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public RigCommandType? PendingCommand { get; set; }
    }
}
