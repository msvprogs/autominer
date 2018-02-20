namespace Msv.AutoMiner.Data.Logic.Contracts
{
    public interface IAutoMinerDbContextFactory
    {
        string ConnectionString { get; }
        AutoMinerDbContext Create();
        AutoMinerDbContext CreateReadOnly();
    }
}
