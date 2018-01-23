using System;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Enums;
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

        public string NetworkApiName { get; set; }

        [Url(ErrorMessage = "Invalid network API URL")]
        public string NetworkApiUrl { get; set; }

        [HexNumber(ErrorMessage = "Max target should be a hex number")]
        public string MaxTarget { get; set; }

        [Required(ErrorMessage = "Algorithm isn't chosen")]
        public Guid? AlgorithmId { get; set; }

        public string RewardCalculationJavaScript { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AddressFormat AddressFormat { get; set; }

        public string AddressPrefixes { get; set; }
    }
}
