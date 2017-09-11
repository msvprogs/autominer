using System.Data.Entity;
using System.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Storage.Contracts;

namespace Msv.AutoMiner.Service.Storage
{
    public class ProfitabilityTableBuilderStorage : IProfitabilityTableBuilderStorage
    {
        public AlgorithmData[] GetAlgorithmDatas()
        {
            using (var context = new AutoMinerDbContext())
                return context.AlgorithmDatas.ToArray();
        }

        public AlgorithmPairData[] GetAlgorithmPairDatas()
        {
            using (var context = new AutoMinerDbContext())
                return context.AlgorithmPairDatas
                    .Include(x => x.Miner)
                    .Include(x => x.Miner.AlgorithmValues)
                    .ToArray();
        }
    }
}
