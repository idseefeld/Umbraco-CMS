using System;
using System.Collections.Generic;
using System.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Our.Umbraco.PostgreSql.EFCore
{
    public class PostgreSqlOptionsExtension : NpgsqlOptionsExtension
    {
        public PostgreSqlOptionsExtension() { }

        protected PostgreSqlOptionsExtension(PostgreSqlOptionsExtension copyFrom)
            : base(copyFrom)
        { }

        protected override NpgsqlOptionsExtension Clone() => new PostgreSqlOptionsExtension(this);
    }
}
