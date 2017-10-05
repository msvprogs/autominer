using System;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Models.Coins;

namespace Msv.AutoMiner.FrontEnd.Models.Pools
{
    public class PoolEditModel : PoolBaseModel
    {
        public PoolApiProtocol ApiProtocol { get; set; }

        [Url(ErrorMessage = "Invalid API URL")]
        public string ApiUrl { get; set; }

        public string ApiKey { get; set; }

        public string ApiPoolName { get; set; }

        public int? ApiPoolUserId { get; set; }

        [Range(0, 100, ErrorMessage = "Pool fee ratio must be between 0% and 100%")]
        public double FeeRatio { get; set; }

        public bool IsAnonymous { get; set; }

        public int Priority { get; set; }

        public string WorkerLogin { get; set; }

        public string WorkerPassword { get; set; }

        [Required(ErrorMessage = "Coin isn't chosen")]
        public Guid? CoinId { get; set; }

        public CoinBaseModel[] AvailableCoins { get; set; }
    }
}
