using System.Runtime.Serialization;
using Our.Umbraco.PostgreSql;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Our.Umbraco.PostgreSql.Services
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
        public string? ServerPlaceholder => "localhost:5433";


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
            var sslMode = databaseModel.TrustServerCertificate ? "SSL Mode=Allow;" : "SSL Mode=VerifyCA;"; // ToDo: SSL Mode should be configurable in appsettings. Read details: https://www.npgsql.org/doc/security.html?tabs=tabid-1

            string connectionString = !string.IsNullOrEmpty(databaseModel.ConnectionString)
                ? databaseModel.ConnectionString
                : $"Host={serverParts[0]};Port={serverParts[1]};Database={databaseModel.DatabaseName};Username={databaseModel.Login};Password={databaseModel.Password};{sslMode}";

            return connectionString;
        }
    }
}
