using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Data
{
    public class RigHeartbeat
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int RigId { get; set; }

        public DateTime Received { get; set; }

        [MaxLength(32768)]
        public string ContentsJson { get; set; }

        [MaxLength(64)]
        public string RemoteAddress { get; set; }
    }
}
