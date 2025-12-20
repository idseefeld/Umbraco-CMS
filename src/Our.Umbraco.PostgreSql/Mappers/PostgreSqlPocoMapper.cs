using System.Data.Common;
using System.Reflection;
using NPoco;
using Umbraco.Extensions;

namespace Our.Umbraco.PostgreSql.Mappers
{
    public class PostgreSqlPocoMapper : DefaultMapper
    {
        public override Func<object, object> GetFromDbConverter(MemberInfo destMemberInfo, Type sourceType)
        {
            var destName = destMemberInfo.DeclaringType?.Name;
            var sourceName = sourceType.Name;

            return base.GetFromDbConverter(destMemberInfo, sourceType);
        }
        public override Func<object, object?> GetFromDbConverter(Type destType, Type sourceType)
        {
            var destName = destType.Name;
            var sourceName = sourceType.Name;

            if (destType == typeof(long))
            {
                return value =>
                {
                    var result = long.Parse($"{value}");
                    return result;
                };
            }

            if (destType == typeof(Guid))
            {
                return value =>
                {
                    var result = Guid.Parse($"{value}");
                    return result;
                };
            }

            if (destType == typeof(Guid?))
            {
                return value =>
                {
                    if (Guid.TryParse($"{value}", out Guid result))
                    {
                        return result;
                    }

                    return default(Guid?);
                };
            }

            return base.GetFromDbConverter(destType, sourceType);
        }

        public override Func<object, object> GetToDbConverter(Type destType, MemberInfo sourceMemberInfo)
        {
            var destName = destType.Name;
            var sourceName = sourceMemberInfo.Name;

            var rVal = base.GetToDbConverter(destType, sourceMemberInfo);
            if (sourceMemberInfo.Name.InvariantContains("date"))
            {

            }

            return rVal;
        }

        public override Func<object, object> GetParameterConverter(DbCommand dbCommand, Type sourceType)
        {
            var sourceName = sourceType.Name;

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
}
