using System.Data.Entity;
using System.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Storage.Contracts;

namespace Msv.AutoMiner.Service.Storage
{
    public class CoinNetworkInfoUpdaterStorage : ICoinNetworkInfoUpdaterStorage
    {
        private readonly string[] m_ExplicitCurrencies;

        public CoinNetworkInfoUpdaterStorage(string[] explicitCurrencies)
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
                    .Include(x => x.Pool.Miner.AlgorithmValues)
                    .Where(x => x.Activity != ActivityState.Deleted);
                if (m_ExplicitCurrencies != null && m_ExplicitCurrencies.Any())
                    query = query.Where(x => m_ExplicitCurrencies.Contains(x.CurrencySymbol));
                return query.ToArray();
            }
        }

        public void SaveCoins(Coin[] coins)
        {
            using (var context = new AutoMinerDbContext())
            {
                var coinIds = coins.Select(x => x.Id).ToArray();
                var existingCoins = context.Coins
                    .Where(x => coinIds.Contains(x.Id))
                    .ToArray();

                foreach (var coin in existingCoins.Join(
                    coins, x => x.Id, x => x.Id, (x, y) => new {Existing = x, New = y}))
                {
                    coin.Existing.Difficulty = coin.New.Difficulty;
                    coin.Existing.NetHashRate = coin.New.NetHashRate;
                    coin.Existing.BlockTimeSeconds = coin.New.BlockTimeSeconds;
                    coin.Existing.BlockReward = coin.New.BlockReward;
                    coin.Existing.Height = coin.New.Height;
                    coin.Existing.StatsUpdated = coin.New.StatsUpdated;
                }
                context.SaveChanges();
            }
        }
    }
}
