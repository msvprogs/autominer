namespace Msv.AutoMiner.Data.Logic
{
    public class AutoMinerDbContextFactory : IAutoMinerDbContextFactory
    {
        private readonly string m_ConnectionString;

        public AutoMinerDbContextFactory(string connectionString)
            => m_ConnectionString = connectionString;

        public AutoMinerDbContext Create()
            => new AutoMinerDbContext(m_ConnectionString);

        public AutoMinerDbContext CreateReadOnly()
            => new AutoMinerDbContext(m_ConnectionString, true);
    }
}