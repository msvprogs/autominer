using System;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msv.AutoMiner.FrontEnd.Models.Coins
{
    public class CoinExportModel : CoinBaseModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public CoinNetworkInfoApiType NetworkInfoApiType { get; set; }

        [MaxLength(64)]
        public string NetworkApiName { get; set; }

        [Url(ErrorMessage = "Invalid network API URL")]
        [MaxLength(512)]
        public string NetworkApiUrl { get; set; }

        [HexNumber(ErrorMessage = "Max target should be a hex number")]
        [MaxLength(128)]
        public string MaxTarget { get; set; }

        [Required(ErrorMessage = "Algorithm isn't chosen")]
        public Guid? AlgorithmId { get; set; }

        [MaxLength(16384)]
        public string RewardCalculationJavaScript { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AddressFormat AddressFormat { get; set; }

        [MaxLength(64)]
        public string AddressPrefixes { get; set; }

        public bool GetDifficultyFromLastPoWBlock { get; set; }
    }
}
