using System.Reflection;
using System.Threading.Tasks;
using Msv.AutoMiner.Common.Data;

namespace Msv.AutoMiner.Common.External.Contracts
{
    [Obfuscation(Exclude = true)]
    public interface IControlCenterService
    {
        Task<ServiceLogs> GetLog();
    }
}
