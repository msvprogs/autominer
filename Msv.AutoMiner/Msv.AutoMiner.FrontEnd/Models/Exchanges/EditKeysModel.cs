using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.Exchanges
{
    public class EditKeysModel
    {
        [HiddenInput]
        public ExchangeType Exchange { get; set; }

        [Required(ErrorMessage = "Public key is required")]
        public string PublicKey { get; set; }

        [Required(ErrorMessage = "Private key is required")]
        public string PrivateKey { get; set; }
    }
}
