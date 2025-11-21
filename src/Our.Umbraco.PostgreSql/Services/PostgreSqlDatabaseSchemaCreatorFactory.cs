using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Our.Umbraco.PostgreSql.Services
{
    public class PostgreSqlDatabaseSchemaCreatorFactory(
        ILogger<DatabaseSchemaCreator> logger,
        ILoggerFactory loggerFactory,
        IUmbracoVersion umbracoVersion,
        IEventAggregator eventAggregator,
        IOptionsMonitor<InstallDefaultDataSettings> installDefaultDataSettings)
        : DatabaseSchemaCreatorFactory(logger, loggerFactory, umbracoVersion, eventAggregator, installDefaultDataSettings)
    {
        public new DatabaseSchemaCreator Create(IUmbracoDatabase? database)
        {
            if (database == null)
            {
                return base.Create(database);
            }

            var db = new PostgreSqlDatabase(
                database.ConnectionString,
                database.SqlContext,
                logger,
                new PostgreSqlBatchSqlInsertProvider(),
                databaseSchemaCreatorFactory: null,
                database.
                );

            return new DatabaseSchemaCreator(
            db,
            logger,
            loggerFactory,
            umbracoVersion,
            eventAggregator,
            installDefaultDataSettings);
        }
    }
