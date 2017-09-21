namespace Msv.AutoMiner.Rig.System.Video
{
    public interface IVideoSystemStateProvider
    {
        bool CanUse { get; }
        VideoSystemState GetState();
    }
}
