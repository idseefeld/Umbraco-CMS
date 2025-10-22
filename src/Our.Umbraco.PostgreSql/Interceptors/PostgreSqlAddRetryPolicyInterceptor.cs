using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.FaultHandling;

namespace Our.Umbraco.PostgreSql.Interceptors
{
    public class PostgreSqlAddRetryPolicyInterceptor : PostgreSqlConnectionInterceptor
    {
        public override System.Data.Common.DbConnection OnConnectionOpened(IDatabase database, System.Data.Common.DbConnection conn)
        {
            {
                RetryStrategy retryStrategy = RetryStrategy.DefaultExponential;
                var commandRetryPolicy = new RetryPolicy(new SqliteTransientErrorDetectionStrategy(), retryStrategy);
                return new RetryDbConnection(conn, null, commandRetryPolicy);
            }
        }
    }
}
