using System;
using MySql.Data.MySqlClient;

namespace Msv.AutoMiner.Common.Data
{
    public static class DatabaseCreator
    {
        public static void CreateIfNotExists(string connectionString)
        {
            if (connectionString == null) 
                throw new ArgumentNullException(nameof(connectionString));

            // Create DB manually to ensure that it will use UTF8 encoding
            var builder = new MySqlConnectionStringBuilder(connectionString);
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
        }
    }
}
