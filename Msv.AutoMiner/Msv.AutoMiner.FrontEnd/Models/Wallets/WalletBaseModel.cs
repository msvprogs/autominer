using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.Common.Enums;

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
    }
}
