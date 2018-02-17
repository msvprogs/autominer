namespace Msv.AutoMiner.ControlCenterService.External.Data
{
    public class PoolCurrencyInfo
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Algorithm { get; set; }
        public int Port { get; set; }
        public int Workers { get; set; }
        public double Hashrate { get; set; }
    }
}
