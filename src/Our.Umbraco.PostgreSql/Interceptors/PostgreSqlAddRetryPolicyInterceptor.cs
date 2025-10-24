using System.Data.Common;
using Microsoft.Extensions.Options;
using NPoco;
using Our.Umbraco.PostgreSql.FaultHandling;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence.FaultHandling;
using Umbraco.Extensions;

namespace Our.Umbraco.PostgreSql.Interceptors
{
    public class PostgreSqlAddRetryPolicyInterceptor : PostgreSqlConnectionInterceptor
    {
        private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;

        public PostgreSqlAddRetryPolicyInterceptor(IOptionsMonitor<ConnectionStrings> connectionStrings)
            => _connectionStrings = connectionStrings;

        public override DbConnection OnConnectionOpened(IDatabase database, DbConnection conn)
        {
            if (!_connectionStrings.CurrentValue.IsConnectionStringConfigured())
            {
                return conn;
            }

            RetryPolicy? connectionRetryPolicy = RetryPolicyFactory.GetPostgreSqlConnectionRetryPolicy();
            RetryPolicy? commandRetryPolicy = RetryPolicyFactory.GetPostgreSqlCommandRetryPolicy();

            if (connectionRetryPolicy == null && commandRetryPolicy == null)
            {
                return conn;
            }

            return new RetryDbConnection(conn, connectionRetryPolicy, commandRetryPolicy);
        }
    }
}
