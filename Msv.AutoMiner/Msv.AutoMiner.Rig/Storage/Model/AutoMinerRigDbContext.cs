﻿using System.Data.Entity;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public class AutoMinerRigDbContext : DbContext
    {
        public DbSet<AlgorithmData> AlgorithmDatas { get; set; }
        public DbSet<Miner> Miners { get; set; }
        public DbSet<MinerAlgorithmSetting> MinerAlgorithmSettings { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<ManualDeviceMapping> ManualDeviceMappings { get; set; }

        static AutoMinerRigDbContext()
            => Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<AutoMinerRigDbContext, ContextMigrationConfiguration>(true));

        public AutoMinerRigDbContext() 
            => Configuration.LazyLoadingEnabled = false;

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MinerAlgorithmSetting>()
                .HasKey(x => new {x.MinerId, x.AlgorithmId});
            modelBuilder.Entity<ManualDeviceMapping>()
                .HasKey(x => new {x.DeviceId, x.DeviceType});
        }
    }
}