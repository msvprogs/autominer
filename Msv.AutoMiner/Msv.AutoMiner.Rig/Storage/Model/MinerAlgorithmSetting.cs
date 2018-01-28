using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public class MinerAlgorithmSetting : IAlgorithmMinerModel
    {
        [MaxLength(64)]
        public string AlgorithmId { get; set; }

        public virtual AlgorithmData Algorithm { get; set; }

        public int MinerId { get; set; }

        public virtual Miner Miner { get; set; }

        public string AlgorithmArgument { get; set; }

        public double? Intensity { get; set; }

        public string LogFile { get; set; }

        public string AdditionalArguments { get; set; }

        [NotMapped]
        Guid IAlgorithmMinerModel.AlgorithmId => string.IsNullOrEmpty(AlgorithmId)
            ? Guid.Empty
            : Guid.Parse(AlgorithmId);

        [NotMapped] 
        string IAlgorithmMinerModel.AlgorithmName => Algorithm?.AlgorithmName;
    }
}
