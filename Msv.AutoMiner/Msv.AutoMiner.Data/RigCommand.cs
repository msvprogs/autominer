using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Data
{
    public class RigCommand
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int RigId { get; set; }

        public RigCommandType Type { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Sent { get; set; }
    }
}
