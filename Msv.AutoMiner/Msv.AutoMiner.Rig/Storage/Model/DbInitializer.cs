using System;
using System.Data.Entity;
using System.Linq;
using SQLite.CodeFirst;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public class DbInitializer : SqliteCreateDatabaseIfNotExists<AutoMinerRigDbContext>
    {
        public DbInitializer(DbModelBuilder modelBuilder) 
            : base(modelBuilder)
        { }

        public override void InitializeDatabase(AutoMinerRigDbContext context)
        {
            base.InitializeDatabase(context);

            if (!context.Miners.Any())
                InitializeMiners(context);
            if (!context.AlgorithmDatas.Any())
                InitializeAlgorithms(context);
            if (!context.MinerAlgorithmSettings.Any())
                InitializeSettings(context);
        }

        private void InitializeSettings(AutoMinerRigDbContext context)
        {
            context.MinerAlgorithmSettings.AddRange(
                new[]
                {
                    new MinerAlgorithmSetting
                    {
                        AlgorithmId = new Guid("007d111d-7649-7f2c-4eeb-c14d011dafe8"),
                        MinerId = context.Miners.First().Id,
                        AlgorithmArgument = "groestl"
                    }
                });
            context.SaveChanges();
        }

        private void InitializeAlgorithms(AutoMinerRigDbContext context)
        {
            context.AlgorithmDatas.AddRange(
                new[]
                {
                    new AlgorithmData
                    {
                        AlgorithmId = new Guid("007d111d-7649-7f2c-4eeb-c14d011dafe8"),
                        AlgorithmName = "Groestl",
                        MinerId = context.Miners.First().Id,
                        Power = 300,
                        SpeedInHashes = 12398129312
                    }
                });
            context.SaveChanges();
        }

        private void InitializeMiners(AutoMinerRigDbContext context)
        {
            context.Miners.AddRange(
                new[]
                {
                    new Miner
                    {
                        Name = "Ccminer Windows",
                        FileName = @"D:\ccminer\ccminer.exe",
                        ServerArgument = "-o",
                        UserArgument = "-u",
                        PasswordArgument = "-p",
                        SpeedRegex = @"(?<=accepted:).*?,\s(?<speed>\d+(\.\d+)?\s[kMGT]H/s)",
                        AlgorithmArgument = "-a",
                        IntensityArgument = "-i",
                        ValidShareRegex = @"accepted: \d+.*? (yes|yay)!"
                    }
                });
            context.SaveChanges();
        }
    }
}
