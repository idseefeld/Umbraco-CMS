using NPoco;
using Umbraco.Extensions;

namespace Our.Umbraco.PostgreSql;

public static class NPocoPostgreSqlDatabaseExtensions
{
    /// <summary>
    ///     Configures NPoco's bulk insert behavior for PostgreSQL.
    ///     Note: NPoco's InsertBulk is optimized for SQL Server only. For PostgreSQL, it will insert records one at a time.
    ///     This method is a placeholder for future PostgreSQL-specific bulk insert logic if needed.
    /// </summary>
    public static void ConfigureNPocoBulkExtensions()
    {
        // No-op for PostgreSQL: NPoco does not support efficient bulk insert for PostgreSQL out of the box.
        // If a PostgreSQL-specific bulk insert implementation is added, configure it here.
    }
}
