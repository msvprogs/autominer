using System.Collections.Generic;
using Msv.AutoMiner.Rig.Storage.Model;

namespace Msv.AutoMiner.Rig.Storage.Contracts
{
    public interface ICommandProcessorStorage
    {
        AlgorithmData[] GetAlgorithms();
        void StoreAlgorithms(AlgorithmData[] newAlgorithms);
        Miner[] GetMiners();
        void SaveMiner(Miner miner);
        MinerAlgorithmSetting[] GetMinerAlgorithmSettings();
        MinerAlgorithmSetting GetMinerAlgorithmSetting(string algorithmName);
        void SaveMinerAlgorithmSetting(MinerAlgorithmSetting setting);
        ManualDeviceMapping[] GetManualMappings();
        void SaveManualMappings(ManualDeviceMapping[] mappings);
        void ClearManualMappings(KeyValuePair<DeviceType, int>[] deviceIds);
    }
}
