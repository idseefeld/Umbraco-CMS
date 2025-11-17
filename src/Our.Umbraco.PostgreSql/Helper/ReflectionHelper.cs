using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Our.Umbraco.PostgreSql.Helper
{
    internal class ReflectionHelper
    {
        public static PropertyInfo? GetInterfaceProperty(Type interfaceType, string propertyName, Type? propertyType = null)
        {
            ArgumentNullException.ThrowIfNull(interfaceType);

            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException("Type must be an interface.", nameof(interfaceType));
            }

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException("Property name is required.", nameof(propertyName));
            }

            PropertyInfo? prop = interfaceType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return prop != null && (propertyType is null || prop.PropertyType == propertyType) ? prop : null;
        }
    }
}
