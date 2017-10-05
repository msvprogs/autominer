using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.FrontEnd.Infrastructure;
using Msv.AutoMiner.FrontEnd.Models.Coins;

namespace Msv.AutoMiner.FrontEnd.Models.Pools
{
    public class PoolBaseModel
    {
        [HiddenInput]
        public int Id { get; set; }

        public CoinBaseModel Coin { get; set; }

        [Required(ErrorMessage = "Pool name is required")]
        public string Name { get; set; }

        [PoolUrl(ErrorMessage = "Pool URL is invalid")]
        [Required(ErrorMessage = "Pool URL is required")]
        public string Url { get; set; }
    }
}
