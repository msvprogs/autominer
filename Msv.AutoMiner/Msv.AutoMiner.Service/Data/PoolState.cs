namespace Msv.AutoMiner.Service.Data
{
    public class PoolState
    {
        public long TotalHashRate { get; set; }
        public int TotalWorkers { get; set; }
        public long? LastBlock { get; set; }
    }
}
