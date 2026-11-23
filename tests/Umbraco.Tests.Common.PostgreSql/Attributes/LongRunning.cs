using NUnit.Framework;

namespace Umbraco.Cms.Tests.Common.PostgreSql.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class LongRunning : CategoryAttribute
{
    public LongRunning()
        : base(TestConstants.Categories.LongRunning)
    {
    }
}
