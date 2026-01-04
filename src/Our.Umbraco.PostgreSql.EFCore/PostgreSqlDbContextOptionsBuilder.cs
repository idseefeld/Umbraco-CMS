using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

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
