using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Our.Umbraco.PostgreSql.Extensions
{
    internal static class DbCommandExtensions
    {
        public static DbCommand FixCommanText(this DbCommand cmd)
        {
            string timeZone = "AT TIME ZONE 'Europe/Berlin' AT TIME ZONE 'UTC'";
            if (cmd.CommandText.Equals("UPDATE umbracoProductLicenseValidationStatus SET LastValidatedOn = LastValidatedOn AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'"))
            {
                cmd.CommandText = $"UPDATE \"umbracoProductLicenseValidationStatus\" SET \"LastValidatedOn\" = \"LastValidatedOn\" {timeZone}";
            }
            else if (cmd.CommandText.Equals("UPDATE umbracoProductLicenseValidationStatus SET LastSuccessfullyValidatedOn = LastSuccessfullyValidatedOn AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'"))
            {
                cmd.CommandText = $"UPDATE \"umbracoProductLicenseValidationStatus\" SET \"LastSuccessfullyValidatedOn\" = \"LastSuccessfullyValidatedOn\" {timeZone}";
            }
            else if (cmd.CommandText.Equals("UPDATE umbracoProductLicenseValidationStatus SET ExpiresOn = ExpiresOn AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'"))
            {
                cmd.CommandText = $"UPDATE \"umbracoProductLicenseValidationStatus\" SET \"ExpiresOn\" = \"ExpiresOn\" {timeZone}";
            }

            if (cmd.CommandText.StartsWith("CREATE ", StringComparison.OrdinalIgnoreCase))
            {
                var text = cmd.CommandText;
            }

            return cmd;
        }
    }
}
