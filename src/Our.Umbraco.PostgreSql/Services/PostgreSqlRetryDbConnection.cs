using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Our.Umbraco.PostgreSql.Extensions;
using Umbraco.Cms.Infrastructure.Persistence.FaultHandling;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Our.Umbraco.PostgreSql.Services
{
    public class PostgreSqlRetryDbConnection : RetryDbConnection
    {
        private readonly RetryPolicy _cmdRetryPolicy;

        public PostgreSqlRetryDbConnection(DbConnection connection, RetryPolicy? conRetryPolicy, RetryPolicy? cmdRetryPolicy)
            : base(connection, conRetryPolicy, cmdRetryPolicy)
        {
            _cmdRetryPolicy = cmdRetryPolicy ?? RetryPolicy.NoRetry;
        }

        protected override DbCommand CreateDbCommand()
        {
            var cmd = Inner.CreateCommand();
            var fixedCmd = cmd.FixCommanText();
            return new PostgreSqlFaultHandlingDbCommand(this, fixedCmd, _cmdRetryPolicy);
        }
    }
}
