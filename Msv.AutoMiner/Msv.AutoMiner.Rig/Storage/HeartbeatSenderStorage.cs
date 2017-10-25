using System.Linq;
using Msv.AutoMiner.Rig.Storage.Contracts;
using Msv.AutoMiner.Rig.Storage.Model;

namespace Msv.AutoMiner.Rig.Storage
{
    public class HeartbeatSenderStorage : IHeartbeatSenderStorage
    {
        public AlgorithmData[] GetAlgorithms()
        {
            using (var context = new AutoMinerRigDbContext())
                return context.AlgorithmDatas.AsNoTracking().ToArray();
        }
    }
}
