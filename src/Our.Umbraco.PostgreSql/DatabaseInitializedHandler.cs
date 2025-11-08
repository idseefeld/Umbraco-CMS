using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Our.Umbraco.PostgreSql.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Our.Umbraco.PostgreSql
{
    public class DatabaseInitializedHandler : INotificationAsyncHandler<DatabaseSchemaAndDataCreatedNotification>, INotificationAsyncHandler<UnattendedInstallNotification>
    {
        //private readonly ILogger<DatabaseInitializedHandler> _logger;
        private readonly IUmbracoDatabaseFactory? _databaseFactory;

        public DatabaseInitializedHandler(
            // ILoggerFactory loggerFactory,
            IUmbracoDatabaseFactory databaseFactory)
        {
            //_logger = loggerFactory.CreateLogger<DatabaseInitializedHandler>();
            _databaseFactory = databaseFactory;
        }

        public async Task HandleAsync(DatabaseSchemaAndDataCreatedNotification notification, CancellationToken cancellationToken)
        {
            if (notification.RequiresUpgrade is false)
            {
                await HandleAsync();
            }
        }

        public async Task HandleAsync(UnattendedInstallNotification notification, CancellationToken cancellationToken)
        {
            await HandleAsync();
        }

        /// <summary>
        /// All PostgreSql sequences need to be updated to current values after initial database creation and data seeding.
        /// </summary>
        private async Task HandleAsync()
        {
            if (_databaseFactory is PostgreSqlDatabaseFactory factory)
            {
                // _logger.LogInformation("Updating PostgreSql sequences to current values after database initialization.");
                factory.AlterSequences();
                // _logger.LogInformation("PostgreSql sequences updated to current values.");
            }
        }
    }
}
