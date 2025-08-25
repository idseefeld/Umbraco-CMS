using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;
using Our.Umbraco.PostgreSql;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Our.Umbraco.PostgreSql.Services
{
    public class PostgreSqlBulkSqlInsertProvider : IBulkSqlInsertProvider
    {
        public string ProviderName => Constants.ProviderName;


        public int BulkInsertRecords<T>(IUmbracoDatabase database, IEnumerable<T> records)
        {
            T[] recordsA = [.. records];
            if (recordsA.Length == 0)
            {
                return 0;
            }

            PocoData? pocoData = database.PocoDataFactory.ForType(typeof(T))
                ?? throw new InvalidOperationException("Could not find PocoData for " + typeof(T));

            return BulkInsertRecordsSqlite(database, pocoData, recordsA);
        }

        /// <summary>
        ///     Bulk-insert records using SqlServer BulkCopy method.
        /// </summary>
        /// <typeparam name="T">The type of the records.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="records">The records.</param>
        /// <returns>The number of records that were inserted.</returns>
        private static int BulkInsertRecordsSqlite<T>(IUmbracoDatabase database, PocoData pocoData, IEnumerable<T> records)
        {
            var count = 0;
            var inTrans = database.InTransaction;

            if (!inTrans)
            {
                database.BeginTransaction();
            }

            string tableName = pocoData.TableInfo.TableName;
            string primaryKeyName = pocoData.TableInfo.PrimaryKey ?? "id";
            foreach (T record in records)
            {
                try
                {
                    _ = database.Insert(tableName, primaryKeyName, record);
                }
                catch (Exception)
                {
                    throw;
                }

                count++;
            }

            if (!inTrans)
            {
                database.CompleteTransaction();
            }

            return count;
        }

        /*
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
        */
    }
}
