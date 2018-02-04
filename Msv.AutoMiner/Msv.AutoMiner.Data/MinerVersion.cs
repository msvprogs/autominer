using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Data
{
    public class MinerVersion : IMinerModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int MinerId { get; set; }

        public PlatformType Platform { get; set; }

        public virtual Miner Miner { get; set; }

        public string Version { get; set; }

        public DateTime Uploaded { get; set; }

        public string ExeFilePath { get; set; }

        public string ExeSecondaryFilePath { get; set; }

        public string ServerArgument { get; set; }

        public string PortArgument { get; set; }

        public string UserArgument { get; set; }

        public string PasswordArgument { get; set; }

        public string IntensityArgument { get; set; }

        public string AlgorithmArgument { get; set; }

        public string BenchmarkArgument { get; set; }

        public string AdditionalArguments { get; set; }

        public string SpeedRegex { get; set; }

        public string ValidShareRegex { get; set; }

        public string InvalidShareRegex { get; set; }

        public string BenchmarkResultRegex { get; set; }

        public string ApiPortArgument { get; set; }

        public MinerApiType ApiType { get; set; }

        public int? ApiPort { get; set; }

        public bool OmitUrlSchema { get; set; }

        public string ZipPath { get; set; }
    
        [NotMapped]
        string IMinerModel.MinerName => Miner.Name;

        [NotMapped]
        int IMinerModel.VersionId => Id;
    }
}