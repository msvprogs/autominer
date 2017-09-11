namespace Msv.AutoMiner.Service.Video
{
    public interface IVideoSystemStateProvider
    {
        bool CanUse { get; }
        VideoSystemState GetState();
    }
}
