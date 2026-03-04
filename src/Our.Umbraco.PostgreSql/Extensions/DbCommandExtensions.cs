using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Our.Umbraco.PostgreSql.Services;
using static System.Net.Mime.MediaTypeNames;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Our.Umbraco.PostgreSql.Extensions
{
    internal static class DbCommandExtensions
    {
        public static DbCommand FixCommanText(this DbCommand cmd, IPackagesService packagesFixService)
        {
            packagesFixService.FixCommanText(cmd);

            if (cmd.CommandText.Contains('['))
            {
                cmd.CommandText = cmd.CommandText.Replace("[", "\"").Replace("]", "\"");
            }

            return cmd;
        }
    }
}
