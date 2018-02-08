using System.Data.Entity.Migrations;
using System.Data.SQLite.EF6.Migrations;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public sealed class ContextMigrationConfiguration : DbMigrationsConfiguration<AutoMinerRigDbContext>
    {
        public ContextMigrationConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            SetSqlGenerator("System.Data.SQLite", new SQLiteMigrationSqlGenerator());
        }
    }
}