using Microsoft.EntityFrameworkCore;

namespace Msv.AutoMiner.Data
{
    public class AutoMinerDbContext : DbContext
    {
        public AutoMinerDbContext()
        { }

        public AutoMinerDbContext(DbContextOptions<AutoMinerDbContext> options)
            : base(options)
        { }

        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<CoinNetworkInfo> CoinNetworkInfos { get; set; }
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
        public DbSet<WalletBalance> WalletBalances { get; set; }
        public DbSet<WalletOperation> WalletOperations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CoinNetworkInfo>()
                .HasKey(x => new {x.CoinId, x.Created});
            modelBuilder.Entity<ExchangeMarketPrice>()
                .HasKey(x => new {x.SourceCoinId, x.TargetCoinId, x.Exchange, x.DateTime});
            modelBuilder.Entity<ExchangeCoin>()
                .HasKey(x => new {x.CoinId, x.Exchange});
            modelBuilder.Entity<CoinFiatValue>()
                .HasKey(x => new {x.CoinId, x.FiatCurrencyId, x.DateTime, x.Source});
            modelBuilder.Entity<PoolAccountState>()
                .HasKey(x => new {x.PoolId, x.DateTime});
            modelBuilder.Entity<PoolPayment>()
                .HasKey(x => new { x.PoolId, x.ExternalId });
            modelBuilder.Entity<WalletBalance>()
                .HasKey(x => new { x.WalletId, x.DateTime });
            modelBuilder.Entity<WalletOperation>()
                .HasKey(x => new { x.WalletId, x.ExternalId });
        }
    }
}