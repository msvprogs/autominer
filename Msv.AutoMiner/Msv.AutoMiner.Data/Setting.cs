using System.ComponentModel.DataAnnotations;

namespace Msv.AutoMiner.Data
{
    public class Setting
    {
        [Key, Required, MaxLength(32)]
        public string Key { get; set; }

        [MaxLength(1024)]
        public string Value { get; set; }
    }
}
