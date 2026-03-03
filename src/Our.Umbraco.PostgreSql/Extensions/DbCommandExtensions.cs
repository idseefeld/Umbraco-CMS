using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Our.Umbraco.PostgreSql.Extensions
{
    internal static class DbCommandExtensions
    {
        public static DbCommand FixCommanText(this DbCommand cmd)
        {
            FixUmbracoLicenseIssues(cmd);

            FixUmbracoFormsIssues(cmd);

            if (cmd.CommandText.Contains('['))
            {
                cmd.CommandText = cmd.CommandText.Replace("[", "\"").Replace("]", "\"");
            }

            return cmd;
        }

        private static void FixUmbracoFormsIssues(DbCommand cmd)
        {
            switch (cmd.CommandText)
            {
                case "SELECT COUNT(*)\nFROM \"UFFolders\"\nWHERE (\"UFFolders\".\"ParentKey\" NOT IN (SELECT \"UFFolders\".\"Key\" AS \"Key\"\nFROM \"UFFolders\"))\nAND (ParentKey Is Not Null)":
                    cmd.CommandText = "SELECT COUNT(*)\nFROM \"UFFolders\"\nWHERE (\"UFFolders\".\"ParentKey\" NOT IN (SELECT \"UFFolders\".\"Key\" AS \"Key\"\nFROM \"UFFolders\"))\nAND (\"ParentKey\" Is Not Null)";
                    break;
                case "SELECT COUNT(*)\nFROM \"UFForms\"\nWHERE (\"UFForms\".\"FolderKey\" NOT IN (SELECT \"UFFolders\".\"Key\" AS \"Key\"\nFROM \"UFFolders\"))\nAND (FolderKey Is Not Null)":
                    cmd.CommandText = "SELECT COUNT(*)\nFROM \"UFForms\"\nWHERE (\"UFForms\".\"FolderKey\" NOT IN (SELECT \"UFFolders\".\"Key\" AS \"Key\"\nFROM \"UFFolders\"))\nAND (\"FolderKey\" Is Not Null)";
                    break;
                case "UPDATE UFDataSource SET Created = Created AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFDataSource\" SET \"Created\" = \"Created\" {GetTimeZone()}";
                    break;
                case "UPDATE UFDataSource SET Updated = Updated AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFDataSource\" SET \"Updated\" = \"Updated\" {GetTimeZone()}";
                    break;
                case "UPDATE UFFolders SET Created = Created AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFFolders\" SET \"Created\" = \"Created\" {GetTimeZone()}";
                    break;
                case "UPDATE UFForms SET Updated = Updated AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFForms\" SET \"Updated\" = \"Updated\" {GetTimeZone()}";
                    break;
                case "UPDATE UFFolders SET Updated = Updated AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFFolders\" SET \"Updated\" = \"Updated\" {GetTimeZone()}";
                    break;
                case "UPDATE UFForms SET Created = Created AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFForms\" SET \"Created\" = \"Created\" {GetTimeZone()}";
                    break;
                case "UPDATE UFPrevalueSource SET Created = Created AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFPrevalueSource\" SET \"Created\" = \"Created\" {GetTimeZone()}";
                    break;
                case "UPDATE UFPrevalueSource SET Updated = Updated AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFPrevalueSource\" SET \"Updated\" = \"Updated\" {GetTimeZone()}";
                    break;
                case "UPDATE UFRecords SET Created = Created AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFRecords\" SET \"Created\" = \"Created\" {GetTimeZone()}";
                    break;
                case "UPDATE UFRecords SET Updated = Updated AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFRecords\" SET \"Updated\" = \"Updated\" {GetTimeZone()}";
                    break;
                case "UPDATE UFRecordAudit SET UpdatedOn = UpdatedOn AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFRecordAudit\" SET \"UpdatedOn\" = \"UpdatedOn\" {GetTimeZone()}";
                    break;
                case "UPDATE UFRecordDataDateTime SET Value = Value AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFRecordDataDateTime\" SET \"Value\" = \"Value\" {GetTimeZone()}";
                    break;
                case "UPDATE UFRecordWorkflowAudit SET ExecutedOn = ExecutedOn AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFRecordWorkflowAudit\" SET \"ExecutedOn\" = \"ExecutedOn\" {GetTimeZone()}";
                    break;
                case "UPDATE UFWorkflows SET Created = Created AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFWorkflows\" SET \"Created\" = \"Created\" {GetTimeZone()}";
                    break;
                case "UPDATE UFWorkflows SET Updated = Updated AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"UFWorkflows\" SET \"Updated\" = \"Updated\" {GetTimeZone()}";
                    break;
            }
        }

        private static void FixUmbracoLicenseIssues(DbCommand cmd)
        {
            switch (cmd.CommandText)
            {
                case "UPDATE umbracoProductLicenseValidationStatus SET LastValidatedOn = LastValidatedOn AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"umbracoProductLicenseValidationStatus\" SET \"LastValidatedOn\" = \"LastValidatedOn\" {GetTimeZone()}";
                    break;
                case "UPDATE umbracoProductLicenseValidationStatus SET LastSuccessfullyValidatedOn = LastSuccessfullyValidatedOn AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"umbracoProductLicenseValidationStatus\" SET \"LastSuccessfullyValidatedOn\" = \"LastSuccessfullyValidatedOn\" {GetTimeZone()}";
                    break;
                case "UPDATE umbracoProductLicenseValidationStatus SET ExpiresOn = ExpiresOn AT TIME ZONE 'W. Europe Standard Time' AT TIME ZONE 'UTC'":
                    cmd.CommandText = $"UPDATE \"umbracoProductLicenseValidationStatus\" SET \"ExpiresOn\" = \"ExpiresOn\" {GetTimeZone()}";
                    break;
            }
        }

        private static string GetTimeZone(string timeZone = "W. Europe Standard Time")
        {
            var tz = "Europe/Berlin";

            if (timeZone.StartsWith("Central European Standard Time", StringComparison.OrdinalIgnoreCase))
            {
                tz = "Europe/Prague";
            }

            return $"AT TIME ZONE '{tz}' AT TIME ZONE 'UTC'";
        }
    }
}
