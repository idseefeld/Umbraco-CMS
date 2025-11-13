using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Our.Umbraco.PostgreSql.Mappers
{
    public class PostgreSqlNullableGuidScalarMapper : ScalarMapper<Guid?>
    {
        protected override Guid? Map(object? value)
        {
            if (value is null || value == DBNull.Value)
            {
                return default;
            }

            try
            {
                return Guid.TryParse($"{value}", out Guid result)
                      ? result
                      : default(Guid?);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
