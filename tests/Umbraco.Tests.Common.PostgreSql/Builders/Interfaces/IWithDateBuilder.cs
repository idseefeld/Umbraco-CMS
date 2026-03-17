using System;

namespace Umbraco.Cms.Tests.Common.PostgreSql.Builders.Interfaces;

public interface IWithDateBuilder
{
    DateTime? Date { get; set; }
}
