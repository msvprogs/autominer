using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Storage.Contracts
{
    public interface ICertificateServiceStorage
    {
        Rig GetRigByName(string name);
    }
}
