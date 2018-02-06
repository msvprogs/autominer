using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.FrontEnd.Infrastructure;

namespace Msv.AutoMiner.FrontEnd.Models.Pools
{
    public class PoolBaseModel
    {
        [HiddenInput]
        public int Id { get; set; }

        [Required(ErrorMessage = "Pool name is required")]
        [MaxLength(128)]
        public string Name { get; set; }

        [PoolUrl(ErrorMessage = "Pool URL is invalid")]
        [Required(ErrorMessage = "Pool URL is required")]
        [MaxLength(128)]
        public string Url { get; set; }
     
        public bool UseBtcWallet { get; set; }
    }
}
