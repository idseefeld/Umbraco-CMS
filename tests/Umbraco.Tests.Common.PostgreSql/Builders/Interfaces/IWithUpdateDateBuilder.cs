// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;

namespace Umbraco.Cms.Tests.Common.PostgreSql.Builders.Interfaces;

public interface IWithUpdateDateBuilder
{
    DateTime? UpdateDate { get; set; }
}
