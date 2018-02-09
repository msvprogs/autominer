using System.Threading.Tasks;
using Msv.AutoMiner.Common.Data;

namespace Msv.AutoMiner.Common.External.Contracts
{
    public interface IControlCenterService
    {
        Task<ServiceLogs> GetLog();
    }
}
