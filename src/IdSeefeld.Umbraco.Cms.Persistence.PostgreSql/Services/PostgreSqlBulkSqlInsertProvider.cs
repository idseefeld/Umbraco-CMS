using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Persistence;

namespace IdSeefeld.Umbraco.Cms.Persistence.PostgreSql.Services
{
    internal class PostgreSqlBulkSqlInsertProvider : IBulkSqlInsertProvider
    {
        public string ProviderName => Constants.ProviderName;

        public int BulkInsertRecords<T>(IUmbracoDatabase database, IEnumerable<T> records) => throw new NotImplementedException();
    }
}
