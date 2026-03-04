using System.Data.Common;
using Our.Umbraco.PostgreSql.Services;

namespace Our.Umbraco.PostgreSql.Umbraco.Forms
{
    /// <summary>
    /// Provides functionality to correct Umbraco.Forms SQL statements and validation timestamp updates in PostgreSQL database commands.
    /// </summary>
    public class PostgreSqlFixUmbracoFormsService : PostgreSqlFixServiceBase
    {
        public override bool FixCommanText(DbCommand cmd) => FixUmbracoLicenseIssues(cmd);

        private static bool FixUmbracoLicenseIssues(DbCommand cmd)
        {
            var success = true;

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
                default:
                    success = false;
                    break;
            }

            return success;
        }
    }
}
