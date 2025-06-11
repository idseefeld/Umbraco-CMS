using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Our.Umbraco.PostgreSql;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Our.Umbraco.PostgreSql.Services
{
    public class PostgreSqlAddMiniProfilerInterceptor : IProviderSpecificInterceptor
    {
        public string ProviderName => Constants.ProviderName;
    }
}
