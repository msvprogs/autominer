namespace Msv.AutoMiner.FrontEnd.Models.Wallets
{
    public class WalletIndexModel
    {
        public WalletDisplayModel[] Wallets { get; set; }

        public double TotalBtc { get; set; }

        public double TotalUsd { get; set; }

        public double TotalAltcoinBtc { get; set; }

        public double TotalAltcoinUsd { get; set; }
    }
}
