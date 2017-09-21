namespace Msv.AutoMiner.ControlCenterService.External.Data
{
    public class PoolAccountInfo
    {
        public int ValidShares { get; set; }
        public int InvalidShares { get; set; }
        public long HashRate { get; set; }
        public double ConfirmedBalance { get; set; }
        public double UnconfirmedBalance { get; set; }
    }
}
