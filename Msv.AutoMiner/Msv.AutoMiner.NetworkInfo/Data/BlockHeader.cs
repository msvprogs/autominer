namespace Msv.AutoMiner.NetworkInfo.Data
{
    public class BlockHeader
    {
        public long Height { get; set; }
        public string Flags { get; set; }
        public string PreviousBlockHash { get; set; }
        public double Difficulty { get; set; }
        public long Time { get; set; }
    }
}
