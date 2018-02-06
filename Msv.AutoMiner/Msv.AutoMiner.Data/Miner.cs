using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data
{
    public class Miner : IEntity<int>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; }

        public ActivityState Activity { get; set; }
    }
}