using Msv.AutoMiner.Rig.Storage.Model;

namespace Msv.AutoMiner.Rig.Storage.Contracts
{
    public interface IMiningProfitabilityTableBuilderStorage
    {
        AlgorithmData[] GetAlgorithmDatas();
        MinerAlgorithmSetting[] GetAlgorithmSettings();
    }
}
