using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;

namespace IdSeefeld.Umbraco.Cms.Persistence.PostgreSql.Services
{
    /// <inheritdoc />
    public class PostgreSqlDistributedLockingMechanism : IDistributedLockingMechanism
    {
        /*
        private ConnectionStrings _connectionStrings;
        private readonly ILogger<PostgreSqlDistributedLockingMechanism> _logger;
        /// <inheritdoc />
        public PostgreSqlDistributedLockingMechanism(
            ILogger<PostgreSqlDistributedLockingMechanism> logger,
           IOptionsMonitor<ConnectionStrings> connectionStrings)
        {
            _connectionStrings = connectionStrings.CurrentValue;
            connectionStrings.OnChange(x => _connectionStrings = x);
            _logger = logger;
        }
        */

        /// <inheritdoc />
        public bool Enabled => false; // _connectionStrings.IsConnectionStringConfigured() && string.Equals(_connectionStrings.ProviderName, Constants.ProviderName, StringComparison.InvariantCultureIgnoreCase);

        /// <inheritdoc />
        public IDistributedLock ReadLock(int lockId, TimeSpan? obtainLockTimeout = null) => throw new NotImplementedException();

        /// <inheritdoc />
        public IDistributedLock WriteLock(int lockId, TimeSpan? obtainLockTimeout = null) => throw new NotImplementedException();
    }
}
