﻿using System;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Data.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msv.AutoMiner.FrontEnd.Models.Algorithms
{
    public class AlgorithmBaseModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name isn't filled")]
        [MaxLength(64)]
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public KnownCoinAlgorithm? KnownValue { get; set; }

        [MaxLength(32)]
        public string MinerAlgorithmArgument { get; set; }
    }
}