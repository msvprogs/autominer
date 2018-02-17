using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.FrontEnd.Infrastructure;

namespace Msv.AutoMiner.FrontEnd.Models.MultiCoinPools
{
    public class MultiCoinPoolEditModel : MultiCoinPoolBaseModel
    {
        [Required(ErrorMessage = "API URL is empty")]
        [Url(ErrorMessage = "API URL is invalid")]
        [MaxLength(256)]
        public string ApiUrl { get; set; }

        [PoolUrl(ErrorMessage = "Mining URL is invalid")]
        [MaxLength(256)]
        public string MiningUrl { get; set; }
    }
}