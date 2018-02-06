using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.FrontEnd.Models.Algorithms;

namespace Msv.AutoMiner.FrontEnd.Models.Coins
{
    public class CoinBaseModel
    {
        [HiddenInput]
        public Guid Id { get; set; }

        public AlgorithmBaseModel Algorithm { get; set; }

        [Required(ErrorMessage = "Coin name is required")]
        [MaxLength(256, ErrorMessage = "Coin name is too long")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Coin symbol is required")]
        [MaxLength(32, ErrorMessage = "Coin symbol is too long")]
        public string Symbol { get; set; }

        [MaxLength(32768, ErrorMessage = "Logo size is too big")]
        public byte[] Logo { get; set; }
    }
}
