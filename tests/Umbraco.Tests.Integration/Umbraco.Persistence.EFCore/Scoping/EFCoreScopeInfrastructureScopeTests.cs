using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
internal sealed class EFCoreScopeInfrastructureScopeTests : UmbracoIntegrationTest
{
    private IEFCoreScopeProvider<TestUmbracoDbContext> EfCoreScopeProvider =>
        GetRequiredService<IEFCoreScopeProvider<TestUmbracoDbContext>>();

    private IScopeProvider InfrastructureScopeProvider =>
        GetRequiredService<IScopeProvider>();

    private EFCoreScopeAccessor<TestUmbracoDbContext> EfCoreScopeAccessor => (EFCoreScopeAccessor<TestUmbracoDbContext>)GetRequiredService<IEFCoreScopeAccessor<TestUmbracoDbContext>>();

    private IScopeAccessor InfrastructureScopeAccessor => GetRequiredService<IScopeAccessor>();

    [Test]
    public void CanCreateNestedInfrastructureScope()
    {
        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            Assert.IsInstanceOf<EFCoreScope<TestUmbracoDbContext>>(scope);
            Assert.IsNotNull(EfCoreScopeAccessor.AmbientScope);
            Assert.IsNotNull(InfrastructureScopeAccessor.AmbientScope);
            Assert.AreSame(scope, EfCoreScopeAccessor.AmbientScope);
            using (var infrastructureScope = InfrastructureScopeProvider.CreateScope())
            {
                Assert.AreSame(infrastructureScope, InfrastructureScopeAccessor.AmbientScope);
            }

            Assert.IsNotNull(InfrastructureScopeAccessor.AmbientScope);
        }

        Assert.IsNull(EfCoreScopeAccessor.AmbientScope);
        Assert.IsNull(InfrastructureScopeAccessor.AmbientScope);
    }

    #region Standard versions
    [Test]
    [Ignore("The raw SQL in this test does not respect SqlSyntax and uses unsupported database types e.g NAVARCHAR which is not supported by PostgreSql.")]
    public async Task? TransactionWithEfCoreScopeAsParent()
    {
        using (IEfCoreScope<TestUmbracoDbContext> parentScope = EfCoreScopeProvider.CreateScope())
        {
            await parentScope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
            });

            // This should be using same transaction, so insert data into table we're creating
            using (IScope childScope = InfrastructureScopeProvider.CreateScope())
            {
                childScope.Database.Execute("INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>(
                    "SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", n);
                childScope.Complete();
            }

            await parentScope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", result);
            });


            parentScope.Complete();
        }

        // Check that its not rolled back
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNotNull(result);
            });
        }
    }

    [Test]
    [Ignore("The raw SQL in this test does not respect SqlSyntax and uses unsupported database types e.g NAVARCHAR which is not supported by PostgreSql.")]
    public async Task? TransactionWithInfrastructureScopeAsParent()
    {
        using (IScope parentScope = InfrastructureScopeProvider.CreateScope())
        {
            parentScope.Database.Execute("CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");

            using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
            {
                await scope.ExecuteWithContextAsync<Task>(async database =>
                {
                    await database.Database.ExecuteSqlAsync($"INSERT INTO tmp3 (id, name) VALUES (1, 'a')");

                    string? result =
                        await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                    Assert.AreEqual("a", result);
                });

                scope.Complete();
            }

            parentScope.Complete();
        }

        // Check that its not rolled back
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNotNull(result);
            });
        }
    }

    [Test]
    [Ignore("The raw SQL in this test does not respect SqlSyntax and uses unsupported database types e.g NAVARCHAR which is not supported by PostgreSql.")]
    public async Task EFCoreAsParent_DontCompleteWhenChildScopeDoesNotComplete()
    {
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
            });
            scope.Complete();
        }

        using (IEfCoreScope<TestUmbracoDbContext> parentScope = EfCoreScopeProvider.CreateScope())
        {
            using (IScope scope = InfrastructureScopeProvider.CreateScope())
            {
                scope.Database.Execute("INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", n);
            }

            await parentScope.ExecuteWithContextAsync<Task>(async database =>
            {
                // Should still be in transaction and not rolled back yet
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", result);
            });

            parentScope.Complete();
        }

        // Check that its rolled back
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                // Should still be in transaction and not rolled back yet
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNull(result);
            });
        }
    }

    [Test]
    [Ignore("The raw SQL in this test does not respect SqlSyntax and uses unsupported database types e.g NAVARCHAR which is not supported by PostgreSql.")]
    public async Task InfrastructureScopeAsParent_DontCompleteWhenChildScopeDoesNotComplete()
    {
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name NVARCHAR(64))");
            });

            scope.Complete();
        }

        using (IScope parentScope = InfrastructureScopeProvider.CreateScope())
        {
            using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
            {
                await scope.ExecuteWithContextAsync<Task>(async database =>
                {
                    await database.Database.ExecuteSqlAsync($"INSERT INTO tmp3 (id, name) VALUES (1, 'a')");

                    string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                    Assert.AreEqual("a", result);
                });

                string n = parentScope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", n);
            }

            parentScope.Complete();
        }

        // Check that its rolled back
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNull(result);
            });
        }
    }
    #endregion

    #region PostgreSQL versions
    [Test]
    public async Task? TransactionWithEfCoreScopeAsParentPostgreSql()
    {
        using (IEfCoreScope<TestUmbracoDbContext> parentScope = EfCoreScopeProvider.CreateScope())
        {
            await parentScope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name TEXT)");
            });

            // This should be using same transaction, so insert data into table we're creating
            using (IScope childScope = InfrastructureScopeProvider.CreateScope())
            {
                childScope.Database.Execute("INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>(
                    "SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", n);
                childScope.Complete();
            }

            await parentScope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", result);
            });


            parentScope.Complete();
        }

        // Check that its not rolled back
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNotNull(result);
            });
        }
    }

    [Test]
    public async Task? TransactionWithInfrastructureScopeAsParentPostgreSql()
    {
        using (IScope parentScope = InfrastructureScopeProvider.CreateScope())
        {
            parentScope.Database.Execute("CREATE TABLE tmp3 (id INT, name TEXT)");

            using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
            {
                await scope.ExecuteWithContextAsync<Task>(async database =>
                {
                    await database.Database.ExecuteSqlAsync($"INSERT INTO tmp3 (id, name) VALUES (1, 'a')");

                    string? result =
                        await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                    Assert.AreEqual("a", result);
                });

                scope.Complete();
            }

            parentScope.Complete();
        }

        // Check that its not rolled back
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNotNull(result);
            });
        }
    }

    [Test]
    public async Task EFCoreAsParent_DontCompleteWhenChildScopeDoesNotCompletePostgreSql()
    {
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name TEXT)");
            });
            scope.Complete();
        }

        using (IEfCoreScope<TestUmbracoDbContext> parentScope = EfCoreScopeProvider.CreateScope())
        {
            using (IScope scope = InfrastructureScopeProvider.CreateScope())
            {
                scope.Database.Execute("INSERT INTO tmp3 (id, name) VALUES (1, 'a')");
                string n = ScopeAccessor.AmbientScope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", n);
            }

            await parentScope.ExecuteWithContextAsync<Task>(async database =>
            {
                // Should still be in transaction and not rolled back yet
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", result);
            });

            parentScope.Complete();
        }

        // Check that its rolled back
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                // Should still be in transaction and not rolled back yet
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNull(result);
            });
        }
    }

    [Test]
    public async Task InfrastructureScopeAsParent_DontCompleteWhenChildScopeDoesNotCompletePostgreSql()
    {
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                await database.Database.ExecuteSqlAsync($"CREATE TABLE tmp3 (id INT, name TEXT)");
            });

            scope.Complete();
        }

        using (IScope parentScope = InfrastructureScopeProvider.CreateScope())
        {
            using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
            {
                await scope.ExecuteWithContextAsync<Task>(async database =>
                {
                    await database.Database.ExecuteSqlAsync($"INSERT INTO tmp3 (id, name) VALUES (1, 'a')");

                    string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                    Assert.AreEqual("a", result);
                });

                string n = parentScope.Database.ExecuteScalar<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.AreEqual("a", n);
            }

            parentScope.Complete();
        }

        // Check that its rolled back
        using (IEfCoreScope<TestUmbracoDbContext> scope = EfCoreScopeProvider.CreateScope())
        {
            await scope.ExecuteWithContextAsync<Task>(async database =>
            {
                string? result = await database.Database.ExecuteScalarAsync<string>("SELECT name FROM tmp3 WHERE id=1");
                Assert.IsNull(result);
            });
        }
    }
    #endregion
}
