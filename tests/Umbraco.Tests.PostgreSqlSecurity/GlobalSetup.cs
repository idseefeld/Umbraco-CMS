using NUnit.Framework;
using Umbraco.Cms.Tests.Integration.Testing;

/// <summary>
///     Assembly-wide teardown that removes the disposable test databases created by the harness.
/// </summary>
[SetUpFixture]
public class GlobalSetup
{
    [OneTimeTearDown]
    public void TearDown() => BaseTestDatabase.Instance?.TearDown();
}
