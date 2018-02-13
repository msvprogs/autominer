using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Data
{
    public class Pool : IEntity<int>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(128)]
        public string Name { get; set; }

        public Guid CoinId { get; set; }

        public virtual Coin Coin { get; set; }

        public int Priority { get; set; }

        public ActivityState Activity { get; set; }

        public PoolProtocol Protocol { get; set; }

        [Required, MaxLength(128)]
        public string Host { get; set; }

        [Required, Range(1, 65535)]
        public int Port { get; set; }

        [MaxLength(128)]
        public string WorkerLogin { get; set; }

        [MaxLength(64)]
        public string WorkerPassword { get; set; }

        public bool IsAnonymous { get; set; }

        public double FeeRatio { get; set; }

        public PoolApiProtocol ApiProtocol { get; set; }

        public int? PoolUserId { get; set; }

        [MaxLength(256)]
        public string ApiKey { get; set; }

        [MaxLength(256)]
        public string ApiUrl { get; set; }

        [MaxLength(64)]
        public string ApiPoolName { get; set; }

        public double TimeZoneCorrectionHours { get; set; }

        public DateTime? ResponsesStoppedDate { get; set; }

        public PoolAvailabilityState Availability { get; set; }

        public bool UseBtcWallet { get; set; }
    }
}
