using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Our.Umbraco.PostgreSql.EFCore
{
    public class PostgreSqlDbContextOptionsBuilder : RelationalDbContextOptionsBuilder<PostgreSqlDbContextOptionsBuilder, PostgreSqlOptionsExtension>
    {

        public PostgreSqlDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
            : base(optionsBuilder)
        {
            optionsBuilder.UseNpgsql();
        }
    }
}
