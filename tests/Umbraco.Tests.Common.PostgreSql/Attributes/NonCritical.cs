using NUnit.Framework;

namespace Umbraco.Cms.Tests.Common.PostgreSql.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class NonCritical : CategoryAttribute
{
    public NonCritical()
        : base(TestConstants.Categories.NonCritical)
    {
    }
}
