﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.Wallets
{
    public class WalletBaseModel
    {
        [HiddenInput]
        public int Id { get; set; }

        [Required(ErrorMessage = "Wallet address is required")]
        [MaxLength(256, ErrorMessage = "Wallet address is too long")]
        public string Address { get; set; }

        public ExchangeType? ExchangeType { get; set; }

        [MaxLength(128, ErrorMessage = "Wallet name is too long")]
        public string Name { get; set; }
    }
}
