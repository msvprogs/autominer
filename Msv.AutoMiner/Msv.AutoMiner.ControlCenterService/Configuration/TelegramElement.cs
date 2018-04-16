namespace Msv.AutoMiner.ControlCenterService.Configuration
{
    public class TelegramElement
    {
        public bool Enabled { get; set; }
        public string Token { get; set; }
        public string[] Subscribers { get; set; }
        public bool UseProxy { get; set; }
        public string ProxyHost { get; set; }
        public ushort ProxyPort { get; set; }
        public bool IsProxyHttps { get; set; }
    }
}
