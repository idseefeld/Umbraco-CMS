using System.Runtime.Serialization;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Infrastructure.Persistence;

namespace IdSeefeld.Umbraco.Cms.Persistence.PostgreSql.Services
{
    [DataContract]
    public class PostgreSqlDatabaseProviderMetadata : IDatabaseProviderMetadata
    {

        /// <inheritdoc />
        public Guid Id => new Guid("d1f8c5b2-3e4f-4b6a-9c7e-8f0c1d2e3f4a");


        /// <inheritdoc />
        public int SortOrder => 5;

        /// <inheritdoc />
        public string DisplayName => "PostgreSQL";

        /// <inheritdoc />
        public string DefaultDatabaseName => Constants.UmbracoDefaultDatabaseName;


        /// <inheritdoc />
        public string? ProviderName => Constants.ProviderName;


        /// <inheritdoc />
        public bool SupportsQuickInstall => false;


        /// <inheritdoc />
        public bool IsAvailable => true;


        /// <inheritdoc />
        public bool RequiresServer => true;


        /// <inheritdoc />
        public string? ServerPlaceholder => "localhost:5432";


        /// <inheritdoc />
        public bool RequiresCredentials => true;

        /// <inheritdoc/>
        public bool SupportsIntegratedAuthentication => false;

        /// <inheritdoc/>
        public bool RequiresConnectionTest => true;

        /// <inheritdoc />
        public bool ForceCreateDatabase => true;

        /// <inheritdoc />
        public string? GenerateConnectionString(DatabaseModel databaseModel)
        {
            var server = !string.IsNullOrEmpty(databaseModel.Server) ? databaseModel.Server : ServerPlaceholder ?? "localhost:5432";
            var serverParts = server.Split([':'], 2);

            string connectionString = !string.IsNullOrEmpty(databaseModel.ConnectionString)
                ? databaseModel.ConnectionString
                : $"Host={serverParts[0]};Port={serverParts[1]};Database={databaseModel.DatabaseName};Username={databaseModel.Login};Password={databaseModel.Password};";

            return connectionString;
        }
    }
}
