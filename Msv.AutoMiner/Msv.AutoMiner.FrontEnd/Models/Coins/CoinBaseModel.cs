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
        [MaxLength(128, ErrorMessage = "Coin name is too long. Max 128 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Coin symbol is required")]
        [MaxLength(16, ErrorMessage = "Coin symbol is too long. Max 16 characters")]
        public string Symbol { get; set; }

        public byte[] Logo { get; set; }
    }
}
