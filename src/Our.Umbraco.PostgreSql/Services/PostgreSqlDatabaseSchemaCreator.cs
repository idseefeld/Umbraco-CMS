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
    public class PostgreSqlDatabaseSchemaCreator(
        IUmbracoDatabase? database,
        ILogger<DatabaseSchemaCreator> logger,
        ILoggerFactory loggerFactory,
        IUmbracoVersion umbracoVersion,
        IEventAggregator eventAggregator,
        IOptionsMonitor<InstallDefaultDataSettings> installDefaultDataSettings)
        : DatabaseSchemaCreator(database, logger, loggerFactory, umbracoVersion, eventAggregator, installDefaultDataSettings)
    {

    }
}
