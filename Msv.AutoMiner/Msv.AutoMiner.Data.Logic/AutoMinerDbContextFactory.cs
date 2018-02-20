using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.Data.Logic
{
    public class AutoMinerDbContextFactory : IAutoMinerDbContextFactory
    { 
        public string ConnectionString { get; }

        public AutoMinerDbContextFactory(string connectionString)
            => ConnectionString = connectionString;

        public AutoMinerDbContext Create()
            => new AutoMinerDbContext(ConnectionString);

        public AutoMinerDbContext CreateReadOnly()
            => new AutoMinerDbContext(ConnectionString, true);
    }
}