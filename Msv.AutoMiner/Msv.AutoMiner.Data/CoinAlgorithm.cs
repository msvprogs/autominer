using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Data
{
    public class CoinAlgorithm : IEntity<Guid>, IAlgorithmMinerModel
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; }

        public ActivityState Activity { get; set; }

        public ProfitabilityFormulaType ProfitabilityFormulaType { get; set; }

        public KnownCoinAlgorithm? KnownValue { get; set; }

        public int? MinerId { get; set; }

        public virtual Miner Miner { get; set; }

        public double? Intensity { get; set; }

        [MaxLength(32)]
        public string AlgorithmArgument { get; set; }
        
        [MaxLength(128)]
        public string AdditionalArguments { get; set; }

        [NotMapped]
        Guid IAlgorithmMinerModel.AlgorithmId => Id;

        [NotMapped]
        int IAlgorithmMinerModel.MinerId => MinerId.GetValueOrDefault();

        [NotMapped]
        string IAlgorithmMinerModel.AlgorithmName => Name;
    }
}