using System.Data.Entity;
using MySql.Data.Entity;

namespace Msv.AutoMiner.Commons.Data
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class AutoMinerDbContext : DbContext
    {
        public DbSet<AlgorithmData> AlgorithmDatas { get; set; }
        public DbSet<AlgorithmPairData> AlgorithmPairDatas { get; set; }
        public DbSet<Coin> Coins { get; set; }
        public DbSet<CoinProfitability> CoinProfitabilities { get; set; }
        public DbSet<Exchange> Exchanges { get; set; }
        public DbSet<Miner> Miners { get; set; }
        public DbSet<MinerSpeed> MinerSpeeds { get; set; }
        public DbSet<MiningChangeEvent> MiningChangeEvents { get; set; }
        public DbSet<Pool> Pools { get; set; }
        public DbSet<MinerAlgorithmValue> MinerAlgorithmValues { get; set; }
        public DbSet<ExchangeAccountBalance> ExchangeAccountBalances { get; set; }
        public DbSet<ExchangeAccountOperation> ExchangeAccountOperations { get; set; }
        public DbSet<PoolAccountState> PoolAccountStates { get; set; }
        public DbSet<PoolPayment> PoolPayments { get; set; }
        public DbSet<CoinBtcPrice> CoinBtcPrices { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Configuration.LazyLoadingEnabled = false;

            base.OnModelCreating(modelBuilder);
        }
    }
}
