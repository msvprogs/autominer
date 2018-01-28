using Msv.AutoMiner.Rig.Storage.Model;

namespace Msv.AutoMiner.Rig.Storage.Contracts
{
    public interface IConfigurationUpdaterStorage
    {
        Miner[] GetMiners();
        MinerAlgorithmSetting[] GetMinerAlgorithmSettings();

        void SaveMiners(Miner[] miners);
        void SaveMinerAlgorithmSettings(MinerAlgorithmSetting[] algorithmSettings);
        void SaveAlgorithms(AlgorithmData[] algorithmDatas);
    }
}
