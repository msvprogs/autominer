namespace Msv.AutoMiner.FrontEnd.Models.Wallets
{
    public class WalletIndexModel
    {
        public WalletDisplayModel[] Wallets { get; set; }

        public double TotalBtc { get; set; }

        public decimal TotalUsd { get; set; }
    }
}
