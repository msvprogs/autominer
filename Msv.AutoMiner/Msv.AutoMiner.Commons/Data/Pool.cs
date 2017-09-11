using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("pools")]
    public class Pool
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ActivityState Activity { get; set; }

        [Required]
        public string Address { get; set; }

        [Required, Range(1, 65535)]
        public int Port { get; set; }

        public string WorkerLogin { get; set; }

        public string WorkerPassword { get; set; }

        public bool IsAnonymous { get; set; }

        public decimal FeeRatio { get; set; }

        public int? MinerId { get; set; }

        public string LogFile { get; set; }

        public PoolApiProtocol ApiProtocol { get; set; }

        public int? PoolUserId { get; set; }

        public string ApiKey { get; set; }

        public string ApiUrl { get; set; }

        public DateTime? ResponsesStoppedDate { get; set; }

        public decimal? Intensity { get; set; }

        public int? DifficultyMultiplier { get; set; }

        public string ApiPoolName { get; set; }

        public PoolProtocol Protocol { get; set; }

        public virtual Miner Miner { get; set; }

        public virtual ICollection<Coin> Coins { get; set; }
    }
}
