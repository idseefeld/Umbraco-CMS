using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Persistence;

namespace IdSeefeld.Umbraco.Cms.Persistence.PostgreSql.Services
{
    /// <summary>
    /// Implements <see cref="IDatabaseCreator" /> for PostgreSQL.
    /// </summary>
    public class PostgreSqlDatabaseCreator : IDatabaseCreator
    {
        private readonly ILogger<PostgreSqlDatabaseCreator> _logger;
        public string ProviderName => Constants.ProviderName;

        public PostgreSqlDatabaseCreator(ILogger<PostgreSqlDatabaseCreator> logger) => _logger = logger;

        public void Create(string connectionString)
        {
            try
            {
                var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
                var databaseName = builder.Database;
                builder.Database = Constants.PostgreSqlDefaultDatabase; // Connect to default db to create new one

                using (var connection = new Npgsql.NpgsqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
                        command.ExecuteNonQuery();
                    }
                }
                _logger.LogInformation("PostgreSql Database '{Database}' created successfully.", databaseName);
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P04") // duplicate_database
            {
                _logger.LogWarning("Database already exists.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the database.");
                throw;
            }
        }
    }
}
