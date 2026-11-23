namespace Umbraco.Cms.Tests.Common.PostgreSql.Builders.Interfaces;

public interface IWithContainerKeyBuilder
{
    Guid? ContainerKey { get; set; }
}
