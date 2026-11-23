// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Tests.Common.PostgreSql.Builders.Interfaces;

public interface IWithFailedPasswordAttemptsBuilder
{
    int? FailedPasswordAttempts { get; set; }
}
