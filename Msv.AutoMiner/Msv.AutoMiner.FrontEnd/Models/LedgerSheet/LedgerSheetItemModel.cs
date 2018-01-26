using Msv.AutoMiner.FrontEnd.Models.Coins;

namespace Msv.AutoMiner.FrontEnd.Models.LedgerSheet
{
    public class LedgerSheetItemModel
    {
        public CoinBaseModel Coin { get; set; }

        public double Debit { get; set; }

        public double Credit { get; set; }

        public double CoinBtcPrice { get; set; }

        public string Address { get; set; }

        public double Balance => Debit - Credit;
    }
}