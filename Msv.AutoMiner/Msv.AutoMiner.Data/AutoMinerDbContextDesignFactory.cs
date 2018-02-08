using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Msv.AutoMiner.Data
{
    //Don't include this file in the release
    #if DEBUG
    public class AutoMinerDbContextDesignFactory : IDesignTimeDbContextFactory<AutoMinerDbContext>
    {
        public AutoMinerDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AutoMinerDbContext>();
            optionsBuilder.UseMySql("Server=192.168.1.154;Database=autominer_srv;Uid=root;Pwd=root;");
            return new AutoMinerDbContext(optionsBuilder.Options);
        }
    }
    #endif
}