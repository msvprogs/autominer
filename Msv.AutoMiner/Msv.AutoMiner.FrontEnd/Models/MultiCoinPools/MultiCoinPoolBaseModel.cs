using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.FrontEnd.Models.MultiCoinPools
{
    public class MultiCoinPoolBaseModel
    {
        [HiddenInput]
        public int Id { get; set; }

        [Required(ErrorMessage = "Multicoin pool name is empty")]
        [MaxLength(64)]
        public string Name { get; set; }

        [Url(ErrorMessage = "Invalid pool site URL")]
        [MaxLength(256)]
        public string SiteUrl { get; set; }

        public PoolApiProtocol ApiProtocol { get; set; }
    }
}
