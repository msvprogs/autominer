namespace Msv.AutoMiner.ControlCenterService.Configuration
{
    public class TelegramElement
    {
        public bool Enabled { get; set; }
        public string Token { get; set; }
        public string[] Subscribers { get; set; }
    }
}
