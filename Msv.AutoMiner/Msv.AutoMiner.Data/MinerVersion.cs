using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Data
{
    public class MinerVersion : IMinerModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int MinerId { get; set; }

        public PlatformType Platform { get; set; }

        public virtual Miner Miner { get; set; }

        [MaxLength(32)]
        public string Version { get; set; }

        public DateTime Uploaded { get; set; }

        [MaxLength(512)]
        public string ExeFilePath { get; set; }

        [MaxLength(512)]
        public string ExeSecondaryFilePath { get; set; }

        [MaxLength(32)]
        public string ServerArgument { get; set; }

        [MaxLength(32)]
        public string PortArgument { get; set; }

        [MaxLength(32)]
        public string UserArgument { get; set; }

        [MaxLength(32)]
        public string PasswordArgument { get; set; }

        [MaxLength(32)]
        public string IntensityArgument { get; set; }

        [MaxLength(32)]
        public string AlgorithmArgument { get; set; }

        [MaxLength(32)]
        public string BenchmarkArgument { get; set; }

        [MaxLength(128)]
        public string AdditionalArguments { get; set; }

        [MaxLength(256)]
        public string SpeedRegex { get; set; }

        [MaxLength(256)]
        public string ValidShareRegex { get; set; }

        [MaxLength(256)]
        public string InvalidShareRegex { get; set; }

        [MaxLength(256)]
        public string BenchmarkResultRegex { get; set; }

        [MaxLength(32)]
        public string ApiPortArgument { get; set; }

        public MinerApiType ApiType { get; set; }

        public int? ApiPort { get; set; }

        public bool OmitUrlSchema { get; set; }

        [MaxLength(512)]
        public string ZipPath { get; set; }
    
        [NotMapped]
        string IMinerModel.MinerName => Miner.Name;

        [NotMapped]
        int IMinerModel.VersionId => Id;
    }
}