using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public class WalletBalanceProvider : IWalletBalanceProvider
    {
        private readonly AutoMinerDbContext m_Context;

        public WalletBalanceProvider(AutoMinerDbContext context)
            => m_Context = context;

        public WalletBalance[] GetLastBalances()
        {
            return m_Context.WalletBalances
                .Include(x => x.Wallet)
                .FromSql(@"SELECT source.* FROM WalletBalances source
  JOIN (SELECT WalletId, MAX(DateTime) AS MaxDateTime FROM WalletBalances
    GROUP BY WalletId) as grouped
  ON source.WalletId = grouped.WalletId AND source.DateTime = grouped.MaxDateTime")
                .ToArray();
        }
    }
}
