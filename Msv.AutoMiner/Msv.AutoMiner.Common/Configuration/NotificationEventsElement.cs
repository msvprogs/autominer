namespace Msv.AutoMiner.Common.Configuration
{
    public class NotificationEventsElement
    {
        public bool CoinNetworkError { get; set; }
        public bool CoinNetworkWarning { get; set; }
        public bool PoolApiError { get; set; }
        public bool PoolConnectionError { get; set; }
    }
}
