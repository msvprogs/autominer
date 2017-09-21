namespace Msv.AutoMiner.ControlCenterService.External.Data
{
    public class PoolInfo
    {
        public PoolAccountInfo AccountInfo { get; set; }
        public PoolState State { get; set; }
        public PoolPaymentData[] PaymentsData { get; set; }
    }
}
