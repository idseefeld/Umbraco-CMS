using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Our.Umbraco.PostgreSql;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Our.Umbraco.PostgreSql.Services
{
    public class PostgreSqlBulkSqlInsertProvider : IBulkSqlInsertProvider
    {
        public string ProviderName => Constants.ProviderName;

        public int BulkInsertRecords<T>(IUmbracoDatabase database, IEnumerable<T> records)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (records == null) throw new ArgumentNullException(nameof(records));

            var recordList = records.ToList();
            if (!recordList.Any())
                return 0;

            // Get table name and columns using PocoData
            var pocoData = database.SqlContext.PocoDataFactory.ForType(typeof(T));
            var tableName = pocoData.TableInfo.TableName;
            var columns = pocoData.Columns
                .Where(c => !c.Value.ResultColumn)
                .Select(c => c.Value.ColumnName)
                .ToList();

            var sb = new StringBuilder();
            sb.Append($"INSERT INTO \"{tableName}\" (");
            sb.Append(string.Join(", ", columns.Select(c => $"\"{c}\"")));
            sb.Append(") VALUES ");

            var paramList = new List<object>();
            int rowIndex = 0;
            foreach (var record in recordList)
            {
                if (rowIndex > 0)
                    sb.Append(", ");

                sb.Append("(");
                sb.Append(string.Join(", ", columns.Select((col, colIndex) =>
                {
                    var paramName = $"@p_{rowIndex}_{colIndex}";
                    paramList.Add(pocoData.Columns[col].GetValue(record));
                    return paramName;
                })));
                sb.Append(")");
                rowIndex++;
            }

            // Build parameterized query
            var sql = sb.ToString();
            var args = paramList.ToArray();

            return database.Execute(sql, args);
        }
    }
}
