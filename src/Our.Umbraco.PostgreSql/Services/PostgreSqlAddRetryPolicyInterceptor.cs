using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Our.Umbraco.PostgreSql.Services
{
    public class PostgreSqlAddRetryPolicyInterceptor : IProviderSpecificInterceptor
    {
        public string ProviderName => Constants.ProviderName;
    }
}
