using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCoreBlogFeatures
{
    public static partial class Constants
    {
        public static class ProviderNames
        {
            public const string SQLLite = "Microsoft.Data.Sqlite";

            public const string SQLServer = "Microsoft.Data.SqlClient";

            public const string PostgreSQL = "Npgsql";
        }
    }
}
