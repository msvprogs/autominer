namespace Msv.AutoMiner.Data.Logic
{
    public interface IAutoMinerDbContextFactory
    {
        AutoMinerDbContext Create();
    }
}
