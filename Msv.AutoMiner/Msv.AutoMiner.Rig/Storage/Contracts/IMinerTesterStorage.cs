using System;
using Msv.AutoMiner.Rig.Storage.Model;

namespace Msv.AutoMiner.Rig.Storage.Contracts
{
    public interface IMinerTesterStorage
    {
        MinerAlgorithmSetting[] GetMinerAlgorithmSettings();
        void StoreAlgorithmData(Guid algorithmId, string algorithmName, double hashRate, double power);
    }
}
