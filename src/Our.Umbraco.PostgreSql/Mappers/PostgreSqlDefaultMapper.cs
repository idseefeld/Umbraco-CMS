using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Our.Umbraco.PostgreSql.Mappers
{
    public class PostgreSqlDefaultMapper : DefaultMapper
    {
        public override Func<object, object> GetToDbConverter(Type destType, MemberInfo sourceMemberInfo)
        {
            var rVal = base.GetToDbConverter(destType, sourceMemberInfo);
            if (sourceMemberInfo.Name.InvariantContains("date"))
            {

            }

            return rVal;
        }

        public override Func<object, object> GetParameterConverter(DbCommand dbCommand, Type sourceType)
        {
            Func<object, object> rVal = base.GetParameterConverter(dbCommand, sourceType);
            if (dbCommand.CommandText != null && dbCommand.CommandText.Contains("parentID"))
            {
                dbCommand.CommandText.Replace("parentID", "parentId");
            }
            return rVal ?? (value =>
            {
                if (value is DateTime dt)
                {
                    // PostgreSQL Npgsql expects DateTime to be in UTC
                    return dt.ToUniversalTime();
                }
                return value;
            });
        }
    }
    public class PostgreSqlGuidScalarMapper : ScalarMapper<Guid>
    {
        protected override Guid Map(object value)
            => Guid.Parse($"{value}");
    }

    public class PostgreSqlNullableGuidScalarMapper : ScalarMapper<Guid?>
    {
        protected override Guid? Map(object? value)
        {
            if (value is null || value == DBNull.Value)
            {
                return default;
            }

            return Guid.TryParse($"{value}", out Guid result)
                ? result
                : default(Guid?);
        }
    }

    public class PostgreSqlDateTimeScalarMapper : ScalarMapper<DateTime>
    {
        protected override DateTime Map(object value)
        {
            if (value is null || value == DBNull.Value)
                return default;

            if (value is DateTime dt)
                return dt;

            // PostgreSQL Npgsql returns timestamp as DateTime or string, depending on mapping
            if (value is string s && DateTime.TryParse(s, out var parsed))
                return parsed;

            throw new InvalidCastException($"Cannot convert value '{value}' of type '{value.GetType()}' to DateTime.");
        }
    }

    public class PostgreSqlNullableDateTimeScalarMapper : ScalarMapper<DateTime?>
    {
        protected override DateTime? Map(object value)
        {
            if (value is null || value == DBNull.Value)
            { return default; }

            if (value is DateTime dt)
            { return dt; }

            // PostgreSQL Npgsql returns timestamp as DateTime or string, depending on mapping
            if (value is string s && DateTime.TryParse(s, out DateTime parsed))
            { return parsed; }

            throw new InvalidCastException($"Cannot convert value '{value}' of type '{value.GetType()}' to DateTime.");
        }
    }
}
