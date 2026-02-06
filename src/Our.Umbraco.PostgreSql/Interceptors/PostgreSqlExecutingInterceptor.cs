using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;
using System.Data.Common;

namespace Our.Umbraco.PostgreSql.Interceptors;

/// <summary>
/// Provider-specific interceptor to customize PostgreSQL command execution.
/// </summary>
public class PostgreSqlExecutingInterceptor : IProviderSpecificExecutingInterceptor
{
    public string ProviderName => Constants.ProviderName;

    /// <summary>
    /// Called before NPoco executes a DbCommand.
    /// </summary>
    public void OnExecutingCommand(IDatabase database, DbCommand command)
    {
        // DateTime conversion is handled in UmbracoPostgreSQLDatabaseType.MapParameterValue
    }

    /// <summary>
    /// Called after execution (both for readers and scalars).
    /// </summary>
    public void OnExecutedCommand(IDatabase database, DbCommand command)
    {
        // Place for diagnostics or lightweight metrics
    }
}
