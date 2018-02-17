using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Data
{
    public class MultiCoinPool : IEntity<int>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public ActivityState Activity { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; }

        [MaxLength(256)]
        public string SiteUrl { get; set; }

        [MaxLength(256)]
        public string MiningUrl { get; set; }

        public PoolApiProtocol ApiProtocol { get; set; }

        [MaxLength(256)]
        public string ApiUrl { get; set; }
    }
}
