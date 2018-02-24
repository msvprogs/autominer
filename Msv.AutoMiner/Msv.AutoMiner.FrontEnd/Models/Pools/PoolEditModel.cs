using System;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Models.Coins;

namespace Msv.AutoMiner.FrontEnd.Models.Pools
{
    public class PoolEditModel : PoolBaseModel
    {
        public PoolApiProtocol PoolApiProtocol { get; set; }

        [Url(ErrorMessage = "Invalid API URL")]
        [MaxLength(256)]
        public string ApiUrl { get; set; }

        [MaxLength(256)]
        public string ApiKey { get; set; }

        [MaxLength(64)]
        public string ApiPoolName { get; set; }

        [Url(ErrorMessage = "Invalid API URL")]
        [MaxLength(256)]
        public string ApiSecondaryUrl { get; set; }

        public int? ApiPoolUserId { get; set; }

        [Range(0, 100, ErrorMessage = "Pool fee ratio must be between 0% and 100%")]
        public double FeeRatio { get; set; }

        public bool IsAnonymous { get; set; }

        public int Priority { get; set; }

        [MaxLength(128)]
        public string WorkerLogin { get; set; }

        [MaxLength(64)]
        public string WorkerPassword { get; set; }

        public double TimeZoneCorrectionHours { get; set; }

        [Required(ErrorMessage = "Coin isn't chosen")]
        public Guid? CoinId { get; set; }

        public CoinBaseModel[] AvailableCoins { get; set; }

        public PoolProtocol PoolProtocol { get; set; }

        public bool ChooseProtocol { get; set; }
    }
}
