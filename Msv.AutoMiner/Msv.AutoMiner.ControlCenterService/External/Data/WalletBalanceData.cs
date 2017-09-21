namespace Msv.AutoMiner.ControlCenterService.External.Data
{
    public class WalletBalanceData
    {
        public string CurrencySymbol { get; set; }
        public string Address { get; set; }
        public double Available { get; set; }
        public double Blocked { get; set; }
        public double Unconfirmed { get; set; }
    }
}
