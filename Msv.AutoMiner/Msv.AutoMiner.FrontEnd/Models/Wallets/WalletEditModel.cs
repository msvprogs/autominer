using System;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.FrontEnd.Models.Coins;

namespace Msv.AutoMiner.FrontEnd.Models.Wallets
{
    public class WalletEditModel : WalletBaseModel
    {
        [Required(ErrorMessage = "Coin isn't chosen")]
        public Guid? CoinId { get; set; }

        public CoinBaseModel[] AvailableCoins { get; set; }

        public ExchangeType[] AvailableExchanges { get; set; }

        public bool SetAsMiningTarget { get; set; }
    }
}
