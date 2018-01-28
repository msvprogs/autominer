using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public interface IConfigurationHasher
    {
        byte[] Calculate(IMinerModel[] miners, IAlgorithmMinerModel[] algorithms);
    }
}
