using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Our.Umbraco.PostgreSql.Services
{
    public class PackagesService : IPackagesService
    {
        private readonly IList<IPostgreSqlFixService> _fixPackageServices;

        public PackagesService(IEnumerable<IPostgreSqlFixService> fixPackageServices)
        {
            _fixPackageServices = fixPackageServices.ToList();
        }

        public DbCommand FixCommanText(DbCommand cmd)
        {
            foreach (IPostgreSqlFixService fix in _fixPackageServices)
            {
                if (fix.FixCommanText(cmd))
                {
                    break;
                }
            }

            return cmd;
        }
    }
}
