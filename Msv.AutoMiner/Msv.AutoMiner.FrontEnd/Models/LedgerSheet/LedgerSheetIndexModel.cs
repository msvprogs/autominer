using System;

namespace Msv.AutoMiner.FrontEnd.Models.LedgerSheet
{
    public class LedgerSheetIndexModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public LedgerSheetItemModel[] Items { get; set; }
        public double TotalDebitBtc { get; set; }
        public double TotalCreditBtc { get; set; }
        public double TotalBalanceBtc => TotalDebitBtc - TotalCreditBtc;
    }
}
