using System;
using System.Data.Entity;
using System.Linq;
using Msv.AutoMiner.Rig.Storage.Contracts;
using Msv.AutoMiner.Rig.Storage.Model;

namespace Msv.AutoMiner.Rig.Storage
{
    public class MinerTesterStorage : IMinerTesterStorage
    {
        public MinerAlgorithmSetting[] GetMinerAlgorithmSettings()
        {
            using (var context = new AutoMinerRigDbContext())
                return context.MinerAlgorithmSettings
                    .Include(x => x.Miner)
                    .AsNoTracking()
                    .ToArray();
        }

        public void StoreAlgorithmData(Miner miner, Guid algorithmId, string algorithmName, long hashRate, double power)
        {
            if (miner == null)
                throw new ArgumentNullException(nameof(miner));

            using (var context = new AutoMinerRigDbContext())
            {
                var data = context.AlgorithmDatas.FirstOrDefault(x => x.AlgorithmId == algorithmId && x.MinerId == miner.Id)
                           ?? context.AlgorithmDatas.Add(new AlgorithmData
                           {
                               AlgorithmId = algorithmId,
                               AlgorithmName = algorithmName,
                               MinerId = miner.Id
                           });
                data.SpeedInHashes = hashRate;
                data.Power = power;
                context.SaveChanges();
            }
        }
    }
}
