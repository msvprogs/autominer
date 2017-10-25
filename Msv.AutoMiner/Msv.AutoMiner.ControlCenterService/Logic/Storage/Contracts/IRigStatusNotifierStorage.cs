using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts
{
    public interface IRigStatusNotifierStorage
    {
        Rig GetRig(int rigId);
        int[] GetReceiverIds(string[] userWhiteList);
    }
}
