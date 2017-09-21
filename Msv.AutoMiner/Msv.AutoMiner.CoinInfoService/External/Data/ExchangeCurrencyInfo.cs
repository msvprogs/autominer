namespace Msv.AutoMiner.CoinInfoService.External.Data
{
    public class ExchangeCurrencyInfo
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string ExternalId { get; set; }
        public bool IsActive { get; set; }
        public double WithdrawalFee { get; set; }
        public double MinWithdrawAmount { get; set; }
    }
}