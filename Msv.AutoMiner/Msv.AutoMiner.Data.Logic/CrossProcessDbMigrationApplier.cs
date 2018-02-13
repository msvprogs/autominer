using System;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Data.Logic.Contracts;
using NLog;

namespace Msv.AutoMiner.Data.Logic
{
    public class CrossProcessDbMigrationApplier
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly IAutoMinerDbContextFactory m_ContextFactory;

        public CrossProcessDbMigrationApplier(IAutoMinerDbContextFactory contextFactory) 
            => m_ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));

        public void ApplyIfAny()
        {
            M_Logger.Info("Checking DB schema...");
            using (var mutex = new Mutex(false, "AutoMinerService_CrossProcessMigrationApplier"))
            {
                mutex.WaitOne();
                try
                {
                    using (var context = m_ContextFactory.Create())
                    {
                        var pendingMigrationsCount = context.Database.GetPendingMigrations().Count();
                        if (pendingMigrationsCount > 0)
                        {
                            M_Logger.Info($"Applying {pendingMigrationsCount} new migrations...");
                            context.Database.Migrate();
                        }
                    }
                }
                catch (Exception ex)
                {
                    M_Logger.Error(ex, "Migration applying failed");
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
                M_Logger.Info("DB schema is now in the latest state");
            }
        }
    }
}
