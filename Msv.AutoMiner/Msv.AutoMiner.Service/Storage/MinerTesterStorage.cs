using System.Data.Entity;
using System.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Storage.Contracts;

namespace Msv.AutoMiner.Service.Storage
{
    public class MinerTesterStorage : IMinerTesterStorage
    {
        private readonly string[] m_ExplicitCurrencies;

        public MinerTesterStorage(string[] explicitCurrencies)
        {
            m_ExplicitCurrencies = explicitCurrencies;
        }

        public Coin[] GetCoins()
        {
            using (var context = new AutoMinerDbContext())
            {
                var query = context.Coins
                    .Include(x => x.Pool)
                    .Include(x => x.Pool.Miner)
                    .Include(x => x.Pool.Miner.AlgorithmValues);
                if (m_ExplicitCurrencies != null && m_ExplicitCurrencies.Any())
                    query = query.Where(x => m_ExplicitCurrencies.Contains(x.CurrencySymbol));
                return query.ToArray();
            }
        }

        public void UpdateAlgorithmHashRate(CoinAlgorithm algorithm, long hashRate, double powerUsage)
        {
            using (var context = new AutoMinerDbContext())
            {
                var algorithmData = context.AlgorithmDatas
                    .FirstOrDefault(x => x.Algorithm == algorithm);
                if (algorithmData == null)
                    context.AlgorithmDatas.Add(new AlgorithmData
                    {
                        Algorithm = algorithm,
                        SpeedInHashes = hashRate,
                        Power = powerUsage
                    });
                else
                {
                    algorithmData.SpeedInHashes = hashRate;
                    algorithmData.Power = powerUsage;
                }
                context.SaveChanges();
            }
        }
    }
}
