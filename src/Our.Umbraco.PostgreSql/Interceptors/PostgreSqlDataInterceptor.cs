using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;
using Our.Umbraco.PostgreSql;

namespace Our.Umbraco.PostgreSql.Interceptors;

/// <summary>
/// Intercepts scalar results and row materialization for PostgreSQL.
/// </summary>
public class PostgreSqlDataInterceptor : IProviderSpecificDataInterceptor
{
    public string ProviderName => Constants.ProviderName;

    public object? OnConvertValue(object? value, Type targetType)
    {
        // Example: convert Int64 -> Int32 when target is Int32 (some PG aggregates return bigint)
        if (value is long l && targetType == typeof(int))
        {
            return checked((int)l);
        }
        return value;
    }

    public void OnIncludeCacheMiss(Type type, string sql, object[] args) { }
    public void OnIncludeLoaded(Type type) { }
    public void OnIterating(IEnumerable<object> data) { }
    public void OnDictionaryLoaded() { }

    public bool OnInserting(IDatabase database, InsertContext insertContext) => throw new NotImplementedException();
    public bool OnUpdating(IDatabase database, UpdateContext updateContext) => throw new NotImplementedException();
    public bool OnDeleting(IDatabase database, DeleteContext deleteContext) => throw new NotImplementedException();
}
