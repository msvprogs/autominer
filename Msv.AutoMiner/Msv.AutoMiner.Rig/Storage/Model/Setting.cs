using System.ComponentModel.DataAnnotations;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public class Setting
    {
        [Key, MaxLength(64)]
        public string Key { get; set; }

        [MaxLength(256)]
        public string Value { get; set; }
    }
}
