using System.ComponentModel.DataAnnotations;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public class ManualDeviceMapping
    {
        public DeviceType DeviceType { get; set; }

        public int DeviceId { get; set; }

        [MaxLength(32)]
        public string CurrencySymbol { get; set; }
    }
}
