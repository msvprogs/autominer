﻿using System;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data
{
    public class CoinAlgorithm : IEntity<Guid>
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ActivityState Activity { get; set; }

        public ProfitabilityFormulaType ProfitabilityFormulaType { get; set; }

        public KnownCoinAlgorithm? KnownValue { get; set; }
    }
}
