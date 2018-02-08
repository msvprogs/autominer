using Microsoft.EntityFrameworkCore;

namespace Msv.AutoMiner.Data
{
    public class AutoMinerDbContext : DbContext
    {
        private readonly string m_ConnectionString;
        private readonly bool m_ReadOnly;

        public AutoMinerDbContext(string connectionString, bool readOnly = false)
        {
            m_ConnectionString = connectionString;
            m_ReadOnly = readOnly;
        }

        public AutoMinerDbContext(DbContextOptions<AutoMinerDbContext> options)
            : base(options)
        { }

        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<CoinNetworkInfo> CoinNetworkInfos { get; set; }
        public DbSet<CoinProfitability> CoinProfitabilities { get; set; }
        public DbSet<Exchange> Exchanges { get; set; }
        public DbSet<ExchangeMarketPrice> ExchangeMarketPrices { get; set; }
        public DbSet<ExchangeCoin> ExchangeCoins { get; set; }
        public DbSet<Coin> Coins { get; set; }
        public DbSet<CoinAlgorithm> CoinAlgorithms { get; set; }
        public DbSet<FiatCurrency> FiatCurrencies { get; set; }
        public DbSet<CoinFiatValue> CoinFiatValues { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Pool> Pools { get; set; }
        public DbSet<PoolAccountState> PoolAccountStates { get; set; }
        public DbSet<PoolPayment> PoolPayments { get; set; }
        public DbSet<Rig> Rigs { get; set; }
        public DbSet<RigCommand> RigCommands { get; set; }
        public DbSet<RigHeartbeat> RigHeartbeats { get; set; }
        public DbSet<RigMiningState> RigMiningStates { get; set; }
        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserLogin> UserLogins { get; set; }
        public DbSet<WalletBalance> WalletBalances { get; set; }
        public DbSet<WalletOperation> WalletOperations { get; set; }
        public DbSet<Miner> Miners { get; set; }
        public DbSet<MinerVersion> MinerVersions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (m_ConnectionString != null)
                optionsBuilder.UseMySql(m_ConnectionString, y => y.CommandTimeout(30));
            if (m_ReadOnly)
                optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CoinNetworkInfo>()
                .HasKey(x => new {x.CoinId, x.Created});
            modelBuilder.Entity<ExchangeMarketPrice>()
                .HasKey(x => new {x.SourceCoinId, x.TargetCoinId, Exchange = x.ExchangeType, x.DateTime});
            modelBuilder.Entity<ExchangeMarketPrice>()
                .HasIndex(x => new {x.SourceCoinId, x.TargetCoinId, x.ExchangeType, x.DateTime});
            modelBuilder.Entity<ExchangeCoin>()
                .HasKey(x => new {x.CoinId, x.Exchange});
            modelBuilder.Entity<CoinFiatValue>()
                .HasKey(x => new {x.CoinId, x.FiatCurrencyId, x.DateTime, x.Source});
            modelBuilder.Entity<PoolAccountState>()
                .HasKey(x => new {x.PoolId, x.DateTime});
            modelBuilder.Entity<PoolPayment>()
                .HasKey(x => new {x.PoolId, x.ExternalId});
            modelBuilder.Entity<WalletBalance>()
                .HasKey(x => new {x.WalletId, x.DateTime});
            modelBuilder.Entity<WalletOperation>()
                .HasKey(x => new {x.WalletId, x.ExternalId});
        }
    }
}