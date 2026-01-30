using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;
using Our.Umbraco.PostgreSql;
using System.Data.Common;

namespace Our.Umbraco.PostgreSql.Interceptors;

/// <summary>
/// Provider-specific interceptor to customize PostgreSQL command execution and scalar mapping.
/// </summary>
public class PostgreSqlExecutingInterceptor : IProviderSpecificExecutingInterceptor
{
    public string ProviderName => Constants.ProviderName;

    // Called before NPoco executes a DbCommand
    public void OnExecutingCommand(IDatabase database, DbCommand command)
    {
        // command?.CommandText = command?.CommandText.Replace("UniqueId", "uniqueId");
    }

    // Called after execution (both for readers and scalars)
    public void OnExecutedCommand(IDatabase database, DbCommand command)
    {
        // Place for diagnostics or lightweight metrics
    }
}
