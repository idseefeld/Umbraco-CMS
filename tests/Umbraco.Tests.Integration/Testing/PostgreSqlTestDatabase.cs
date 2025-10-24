using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Our.Umbraco.PostgreSql;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.Integration.Testing
{
    public class PostgreSqlTestDatabase : BaseTestDatabase, ITestDatabase
    {
        public const string DatabaseName = "UmbracoTests";
        private readonly TestDatabaseSettings _settings;

#pragma warning disable IDE1006 // Naming Styles
        protected UmbracoDatabase.CommandInfo[] _cachedDatabaseInitCommands = new UmbracoDatabase.CommandInfo[0];
#pragma warning restore IDE1006 // Naming Styles

        protected override void ResetTestDatabase(TestDbMeta meta)
        {
            using var connection = GetConnection(meta);
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                // Reset schema by dropping and recreating the public schema
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"DROP SCHEMA IF EXISTS public CASCADE; CREATE SCHEMA public;";

                // rudimentary retry policy since a db can still be in use when we try to drop objects
                Retry(10, () => cmd.ExecuteNonQuery());
            }
        }

        protected override DbConnection GetConnection(TestDbMeta meta) => new NpgsqlConnection(meta.ConnectionString);

        protected override void RebuildSchema(IDbCommand command, TestDbMeta meta)
        {
            lock (_cachedDatabaseInitCommands)
            {
                if (!_cachedDatabaseInitCommands.Any())
                {
                    RebuildSchemaFirstTime(meta);
                    return;
                }
            }

            foreach (var dbCommand in _cachedDatabaseInitCommands)
            {
                command.CommandText = dbCommand.Text;
                command.Parameters.Clear();

                foreach (var parameterInfo in dbCommand.Parameters)
                {
                    AddParameter(command, parameterInfo);
                }

                command.ExecuteNonQuery();
            }
        }

        private void RebuildSchemaFirstTime(TestDbMeta meta)
        {
            _databaseFactory.Configure(meta.ToStronglyTypedConnectionString());

            using (var database = (UmbracoDatabase)_databaseFactory.CreateDatabase())
            {
                database.LogCommands = true;

                using (var transaction = database.GetTransaction())
                {
                    var options =
                    new TestOptionsMonitor<InstallDefaultDataSettings>(
                        new InstallDefaultDataSettings { InstallData = InstallDefaultDataOption.All });

                    var schemaCreator = new DatabaseSchemaCreator(
                        database,
                        _loggerFactory.CreateLogger<DatabaseSchemaCreator>(),
                        _loggerFactory,
                        new UmbracoVersion(),
                        Mock.Of<IEventAggregator>(),
                        options);

                    schemaCreator.InitializeDatabaseSchema();

                    transaction.Complete();

                    _cachedDatabaseInitCommands = database.Commands
                        .Where(x => !x.Text.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                }
            }
        }

        public PostgreSqlTestDatabase(TestDatabaseSettings settings, ILoggerFactory loggerFactory, IUmbracoDatabaseFactory databaseFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _databaseFactory = databaseFactory ?? throw new ArgumentNullException(nameof(databaseFactory));
            _settings = settings;
            var counter = 0;

            // Build database metas with correct Npgsql provider and per-db connection strings
            var schema = Enumerable.Range(0, _settings.SchemaDatabaseCount)
                .Select(_ => CreateMeta($"{DatabaseName}-{++counter}", isEmpty: false));
            var empty = Enumerable.Range(0, _settings.EmptyDatabasesCount)
                .Select(_ => CreateMeta($"{DatabaseName}-{++counter}", isEmpty: true));
            _testDatabases = schema.Concat(empty).ToList();
        }

        private TestDbMeta CreateMeta(string name, bool isEmpty)
        {
            // Start from master connection string and override Database
            var builder = new NpgsqlConnectionStringBuilder(_settings.TestMasterConnectionString)
            {
                Database = name
            };
            var connStr = builder.ToString();
            return new TestDbMeta(name, isEmpty, connStr, Constants.ProviderName, path: null);
        }

        protected override void Initialize()
        {
            _prepareQueue = new BlockingCollection<TestDbMeta>();
            _readySchemaQueue = new BlockingCollection<TestDbMeta>();
            _readyEmptyQueue = new BlockingCollection<TestDbMeta>();

            foreach (var meta in _testDatabases)
            {
                CreateDatabase(meta);
                _prepareQueue.Add(meta);
            }

            for (var i = 0; i < _settings.PrepareThreadCount; i++)
            {
                var thread = new Thread(PrepareDatabase);
                thread.Start();
            }
        }

        private void CreateDatabase(TestDbMeta meta)
        {
            Drop(meta);

            using (var connection = new NpgsqlConnection(_settings.TestMasterConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = $"CREATE DATABASE \"{meta.Name}\"";
                    command.ExecuteNonQuery();
                }
            }
        }

        private void Drop(TestDbMeta meta)
        {
            using (var connection = new NpgsqlConnection(_settings.TestMasterConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;

                    // Check if database exists
                    command.CommandText = "SELECT 1 FROM pg_database WHERE datname = @name";
                    command.Parameters.Clear();
                    var p = command.CreateParameter();
                    p.ParameterName = "@name";
                    p.Value = meta.Name;
                    command.Parameters.Add(p);
                    var existsObj = command.ExecuteScalar();
                    var exists = existsObj != null && existsObj != DBNull.Value;
                    if (!exists)
                    {
                        return;
                    }

                    // Terminate existing connections to the database
                    command.Parameters.Clear();
                    command.CommandText = @"SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = @dbname AND pid <> pg_backend_pid();";
                    var p2 = command.CreateParameter();
                    p2.ParameterName = "@dbname";
                    p2.Value = meta.Name;
                    command.Parameters.Add(p2);
                    command.ExecuteNonQuery();

                    // Drop the database
                    command.Parameters.Clear();
                    command.CommandText = $"DROP DATABASE IF EXISTS \"{meta.Name}\"";
                    command.ExecuteNonQuery();
                }
            }
        }

        public override void TearDown()
        {
            if (_prepareQueue == null)
            {
                return;
            }

            _prepareQueue.CompleteAdding();
            while (_prepareQueue.TryTake(out _))
            {
            }

            _readyEmptyQueue.CompleteAdding();
            while (_readyEmptyQueue.TryTake(out _))
            {
            }

            _readySchemaQueue.CompleteAdding();
            while (_readySchemaQueue.TryTake(out _))
            {
            }

            Parallel.ForEach(_testDatabases, Drop);
        }
    }
}
