namespace Msv.AutoMiner.ControlCenterService.Logic.Notifiers
{
    public interface IRigStatusNotifier
    {
        void NotifyLowVideoUsage(int rigId, int[] samples);
        void NotifyHighVideoTemperature(int rigId, int[] samples);
        void NotifyHighInvalidShareRate(int rigId, int[] samples);
        void NotifyUnusualHashRate(int rigId, int[] samples);
    }
}
