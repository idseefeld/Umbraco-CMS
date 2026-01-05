namespace Umbraco.Cms.Tests.Common.PostgreSql.Builders.Interfaces;

public interface IWithDefaultTemplateKeyBuilder
{
    Guid? DefaultTemplateKey { get; set; }
}
