namespace Msv.AutoMiner.FrontEnd.Models.MultiCoinPools
{
    public class MultiCoinPoolCurrencyModel
    {
        public int Id { get; set; }
        public int MultiCoinPoolId { get; set; }
        public string MultiCoinPoolName { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Algorithm { get; set; }
        public string MiningUrl { get; set; }
        public int Workers { get; set; }
        public double Hashrate { get; set; }
        public string[] Exchanges { get; set; }
    }
}
