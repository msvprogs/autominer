namespace Msv.AutoMiner.Data.Logic.Contracts
{
    public interface IAutoMinerDbContextFactory
    {
        AutoMinerDbContext Create();
        AutoMinerDbContext CreateReadOnly();
    }
}
