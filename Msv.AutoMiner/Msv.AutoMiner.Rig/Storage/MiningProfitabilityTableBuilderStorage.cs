using System.Data.Entity;
using System.Linq;
using Msv.AutoMiner.Rig.Storage.Contracts;
using Msv.AutoMiner.Rig.Storage.Model;

namespace Msv.AutoMiner.Rig.Storage
{
    public class MiningProfitabilityTableBuilderStorage : IMiningProfitabilityTableBuilderStorage
    {
        public AlgorithmData[] GetAlgorithmDatas()
        {
            using (var context = new AutoMinerRigDbContext())
                return context.AlgorithmDatas.ToArray();
        }

        public MinerAlgorithmSetting[] GetAlgorithmSettings()
        {
            using (var context = new AutoMinerRigDbContext())
                return context.MinerAlgorithmSettings
                    .Include(x => x.Miner)
                    .ToArray();
        }
    }
}
