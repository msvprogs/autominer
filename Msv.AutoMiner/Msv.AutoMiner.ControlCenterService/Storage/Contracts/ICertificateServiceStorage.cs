using System.Threading.Tasks;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Storage.Contracts
{
    public interface ICertificateServiceStorage
    {
        Task<Rig> GetRigByName(string name);
    }
}
