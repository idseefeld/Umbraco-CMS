using AutoFixture;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.AutoFixture.Customizations;

internal sealed class OmitRecursionCustomization : ICustomization
{
    public void Customize(IFixture fixture) =>
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
}
