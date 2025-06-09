using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdSeefeld.Umbraco.Cms.Persistence.PostgreSql.Mappers;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Persistence;

namespace IdSeefeld.Umbraco.Cms.Persistence.PostgreSql.Services
{
    public class PostgreSqlSpecificMapperFactory : IProviderSpecificMapperFactory
    {
        /// <inheritdoc />
        public string ProviderName => Constants.ProviderName;

        /// <inheritdoc />
        public NPocoMapperCollection Mappers => new(() => [new PostgreSqlDefaultMapper()]);
    }
}
