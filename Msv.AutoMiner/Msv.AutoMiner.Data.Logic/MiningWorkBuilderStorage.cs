using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.Data.Logic
{
    public class MiningWorkBuilderStorage : IMiningWorkBuilderStorage
    {
        private readonly IAutoMinerDbContextFactory m_ContextFactory;

        public MiningWorkBuilderStorage(IAutoMinerDbContextFactory contextFactory) 
            => m_ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

        public Wallet GetBitCoinMiningTarget()
        {
            using (var context = m_ContextFactory.CreateReadOnly())
                return context.Wallets.FirstOrDefault(x => x.IsMiningTarget && x.Coin.Symbol == "BTC");
        }

        public Pool[] GetActivePools(Guid[] coinIds)
        {
            if (coinIds == null)
                throw new ArgumentNullException(nameof(coinIds));

            using (var context = m_ContextFactory.CreateReadOnly())
            {
                return context.Pools
                    .Include(x => x.Coin)
                    .Include(x => x.Coin.Wallets)
                    .Where(x => coinIds.Contains(x.CoinId) && x.Activity == ActivityState.Active)
                    .ToArray();
            }
        }
    }
}
