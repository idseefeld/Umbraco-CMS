using System.Collections;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Our.Umbraco.PostgreSql.EFCore
{
    /// <inheritdoc />
    public class PostgreSqlMigrationCommandInterceptor : IDbCommandInterceptor
    {
        /// <inheritdoc />
        public ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
        {
            return new(result);
        }

        /// <inheritdoc />
        public Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            try
            {
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.CompletedTask;
            }
        }

        /// <inheritdoc />
        public ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
        {
            return new(result);
        }

        /// <inheritdoc />
        public InterceptionResult<DbCommand> CommandCreating(CommandCorrelatedEventData eventData, InterceptionResult<DbCommand> result)
        {
            return result;
        }
    }
}
