using System;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Data.Logic.Contracts;
using MySql.Data.MySqlClient;
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
                    // Create DB manually to ensure that it will use UTF8 encoding
                    var builder = new MySqlConnectionStringBuilder(m_ContextFactory.ConnectionString);
                    var databaseName = builder.Database.Replace('`', ' ');
                    builder.Database = null;
                    using (var connection = new MySqlConnection(builder.ConnectionString))
                    {
                        connection.Open();
                        using (var command = new MySqlCommand(
                            $"CREATE DATABASE IF NOT EXISTS `{databaseName}` CHARACTER SET utf8 COLLATE utf8_unicode_ci;",
                            connection))
                            command.ExecuteNonQuery();
                    }

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
