using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Npgsql;
using NPoco;
using Our.Umbraco.PostgreSql.Services;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Our.Umbraco.PostgreSql.Interceptors
{
    public class PostgreSqlAddDbConnectionInterceptor : PostgreSqlConnectionInterceptor
    {        public override DbConnection OnConnectionOpened(IDatabase database, DbConnection conn)
        => new PostgreSqlAddDbConnection(conn as NpgsqlConnection ?? throw new InvalidOperationException());
    }
}
