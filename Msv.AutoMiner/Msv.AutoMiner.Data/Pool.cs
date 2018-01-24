using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data
{
    public class Pool : IEntity<int>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public Guid CoinId { get; set; }

        public virtual Coin Coin { get; set; }

        public int Priority { get; set; }

        public ActivityState Activity { get; set; }

        public PoolProtocol Protocol { get; set; }

        [Required]
        public string Host { get; set; }

        [Required, Range(1, 65535)]
        public int Port { get; set; }

        public string WorkerLogin { get; set; }

        public string WorkerPassword { get; set; }

        public bool IsAnonymous { get; set; }

        public double FeeRatio { get; set; }

        public PoolApiProtocol ApiProtocol { get; set; }

        public int? PoolUserId { get; set; }

        public string ApiKey { get; set; }

        public string ApiUrl { get; set; }

        public string ApiPoolName { get; set; }

        public double TimeZoneCorrectionHours { get; set; }

        public DateTime? ResponsesStoppedDate { get; set; }

        public bool UseBtcWallet { get; set; }
    }
}
