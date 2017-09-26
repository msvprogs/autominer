using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Notifiers
{
    public interface IRigStatusNotifier
    {
        void NotifyLowVideoUsage(Rig rig);
        void NotifyHighVideoTemperature(Rig rig);
        void NotifyHighInvalidShareRate(Rig rig);
        void NotifyUnusualHashRate(Rig rig);
    }
}
