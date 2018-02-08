namespace Msv.AutoMiner.ControlCenterService.Logic.Notifiers
{
    public class DummyNotifier : INotifier
    {
        public void NotifyLowVideoUsage(int rigId, int[] samples)
        { }

        public void NotifyHighVideoTemperature(int rigId, int[] samples)
        { }

        public void NotifyHighInvalidShareRate(int rigId, int[] samples)
        { }

        public void NotifyUnusualHashRate(int rigId, int[] samples)
        { }

        public void SendMessage(string message)
        { }
    }
}
