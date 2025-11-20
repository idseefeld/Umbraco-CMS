using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using Our.Umbraco.PostgreSql.Services;
using Umbraco.Cms.Infrastructure.Persistence.FaultHandling;

namespace Our.Umbraco.PostgreSql.FaultHandling.Strategies
{
    public class PostgreSqlTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        public bool IsTransient(Exception ex)
        {
            if (ex is not NpgsqlException postgreSqlException)
            {
                return false;
            }

            return postgreSqlException.IsTransient || postgreSqlException.IsBusyOrLocked();
        }
    }
}
