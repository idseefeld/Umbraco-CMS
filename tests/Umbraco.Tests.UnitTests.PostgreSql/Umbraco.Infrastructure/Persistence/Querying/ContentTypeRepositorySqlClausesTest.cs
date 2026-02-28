// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.UnitTests.PostgreSql.TestHelpers;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Infrastructure.Persistence.Querying;

[TestFixture]
public class ContentTypeRepositorySqlClausesTest : BaseUsingPostgreSqlSyntax
{
    private readonly string escapeChar = Our.Umbraco.PostgreSql.Constants.EscapeTableColumAliasNames ? "\"" : string.Empty;
    [Test]
    public void Can_Verify_Base_Clause()
    {
        var nodeObjectType = Constants.ObjectTypes.DocumentType;

        var expected = Sql();
        expected.Select($"*")
            .From($"{escapeChar}cmsDocumentType{escapeChar}")
            .RightJoin($"{escapeChar}cmsContentType{escapeChar}")
            .On($"{escapeChar}cmsContentType{escapeChar}.{escapeChar}nodeId{escapeChar} = {escapeChar}cmsDocumentType{escapeChar}.{escapeChar}contentTypeNodeId{escapeChar}")
            .InnerJoin($"{escapeChar}umbracoNode{escapeChar}")
            .On($"{escapeChar}cmsContentType{escapeChar}.{escapeChar}nodeId{escapeChar} = {escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar}")
            .Where($"({escapeChar}umbracoNode{escapeChar}.{escapeChar}nodeObjectType{escapeChar} = @0)", new Guid($"a2cb7800-f571-4787-9638-bc48539a0efb"))
            .Where($"({escapeChar}cmsDocumentType{escapeChar}.{escapeChar}IsDefault{escapeChar} = @0)", true);

        var sql = Sql();
        sql.SelectAll()
            .From<ContentTypeTemplateDto>()
            .RightJoin<ContentTypeDto>()
            .On<ContentTypeDto, ContentTypeTemplateDto>(left => left.NodeId, right => right.ContentTypeNodeId)
            .InnerJoin<NodeDto>()
            .On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == nodeObjectType)
            .Where<ContentTypeTemplateDto>(x => x.IsDefault == true);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
        for (var i = 0; i < expected.Arguments.Length; i++)
        {
            Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
        }

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_Verify_Base_Where_Clause()
    {
        var nodeObjectType = Constants.ObjectTypes.DocumentType;

        var expected = Sql();
        expected.SelectAll()
            .From($"{escapeChar}cmsDocumentType{escapeChar}")
            .RightJoin($"{escapeChar}cmsContentType{escapeChar}")
            .On($"{escapeChar}cmsContentType{escapeChar}.{escapeChar}nodeId{escapeChar} = {escapeChar}cmsDocumentType{escapeChar}.{escapeChar}contentTypeNodeId{escapeChar}")
            .InnerJoin($"{escapeChar}umbracoNode{escapeChar}")
            .On($"{escapeChar}cmsContentType{escapeChar}.{escapeChar}nodeId{escapeChar} = {escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar}")
            .Where($"({escapeChar}umbracoNode{escapeChar}.{escapeChar}nodeObjectType{escapeChar} = @0)", new Guid($"a2cb7800-f571-4787-9638-bc48539a0efb"))
            .Where($"{escapeChar}cmsDocumentType{escapeChar}.{escapeChar}IsDefault{escapeChar} = @0", true)
            .Where($"({escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar} = @0)", 1050);

        var sql = Sql();
        sql.SelectAll()
            .From<ContentTypeTemplateDto>()
            .RightJoin<ContentTypeDto>()
            .On<ContentTypeDto, ContentTypeTemplateDto>(left => left.NodeId, right => right.ContentTypeNodeId)
            .InnerJoin<NodeDto>()
            .On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == nodeObjectType)
            .Where<ContentTypeTemplateDto>(x => x.IsDefault)
            .Where<NodeDto>(x => x.NodeId == 1050);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
        for (var i = 0; i < expected.Arguments.Length; i++)
        {
            Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
        }

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_Verify_PerformQuery_Clause()
    {
        var expected = Sql();
        expected.SelectAll()
            .From($"{escapeChar}cmsPropertyTypeGroup{escapeChar}")
            .RightJoin($"{escapeChar}cmsPropertyType{escapeChar}").On($"{escapeChar}cmsPropertyTypeGroup{escapeChar}.{escapeChar}id{escapeChar} = {escapeChar}cmsPropertyType{escapeChar}.{escapeChar}propertyTypeGroupId{escapeChar}")
            .InnerJoin($"{escapeChar}{Constants.DatabaseSchema.Tables.DataType}{escapeChar}")
            .On($"{escapeChar}cmsPropertyType{escapeChar}.{escapeChar}dataTypeId{escapeChar} = {escapeChar}{Constants.DatabaseSchema.Tables.DataType}{escapeChar}.{escapeChar}nodeId{escapeChar}");

        var sql = Sql();
        sql.SelectAll()
            .From<PropertyTypeGroupDto>()
            .RightJoin<PropertyTypeDto>()
            .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
            .InnerJoin<DataTypeDto>()
            .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.NodeId);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_Verify_AllowedContentTypeIds_Clause()
    {
        var expected = Sql();
        expected.SelectAll()
            .From($"{escapeChar}cmsContentTypeAllowedContentType{escapeChar}")
            .Where($"({escapeChar}cmsContentTypeAllowedContentType{escapeChar}.{escapeChar}Id{escapeChar} = @0)", 1050);

        var sql = Sql();
        sql.SelectAll()
            .From<ContentTypeAllowedContentTypeDto>()
            .Where<ContentTypeAllowedContentTypeDto>(x => x.Id == 1050);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
        for (var i = 0; i < expected.Arguments.Length; i++)
        {
            Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
        }

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_Verify_PropertyGroupCollection_Clause()
    {
        var expected = Sql();
        expected.SelectAll()
            .From($"{escapeChar}cmsPropertyTypeGroup{escapeChar}")
            .RightJoin($"{escapeChar}cmsPropertyType{escapeChar}").On($"{escapeChar}cmsPropertyTypeGroup{escapeChar}.{escapeChar}id{escapeChar} = {escapeChar}cmsPropertyType{escapeChar}.{escapeChar}propertyTypeGroupId{escapeChar}")
            .InnerJoin($"{escapeChar}{Constants.DatabaseSchema.Tables.DataType}{escapeChar}")
            .On($"{escapeChar}cmsPropertyType{escapeChar}.{escapeChar}dataTypeId{escapeChar} = {escapeChar}{Constants.DatabaseSchema.Tables.DataType}{escapeChar}.{escapeChar}nodeId{escapeChar}")
            .Where($"({escapeChar}cmsPropertyType{escapeChar}.{escapeChar}contentTypeId{escapeChar} = @0)", 1050);

        var sql = Sql();
        sql.SelectAll()
            .From<PropertyTypeGroupDto>()
            .RightJoin<PropertyTypeDto>()
            .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
            .InnerJoin<DataTypeDto>()
            .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.NodeId)
            .Where<PropertyTypeDto>(x => x.ContentTypeId == 1050);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
        for (var i = 0; i < expected.Arguments.Length; i++)
        {
            Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
        }

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_Verify_WhereLike_Clause()
    {
        var key = Guid.NewGuid();
        var propertyTypeId = 1234;
        Sql<ISqlContext> sql = Sql()
            .Delete<UserGroup2GranularPermissionDto>()
            .Where<UserGroup2GranularPermissionDto>(c => c.UniqueId == key)
            .WhereLike<UserGroup2GranularPermissionDto>(
                c => c.Permission,
                Sql()
                    .SelectClosure<PropertyTypeDto>(c => c.ConvertUniqueIdentifierToString(x => x.UniqueId))
                    .From<PropertyTypeDto>()
                    .WhereClosure<PropertyTypeDto>(c => c.Id == propertyTypeId),
                $"'|{SqlContext.SqlSyntax.GetWildcardPlaceholder()}'");

        string expectedSQL =
@$"DELETE FROM {escapeChar}umbracoUserGroup2GranularPermission{escapeChar}
WHERE (({escapeChar}umbracoUserGroup2GranularPermission{escapeChar}.{escapeChar}uniqueId{escapeChar} = @0))
AND ({escapeChar}umbracoUserGroup2GranularPermission{escapeChar}.{escapeChar}permission{escapeChar} LIKE ((SELECT 
 CAST({escapeChar}UniqueId{escapeChar} AS NATIONAL CHARACTER VARYING(36))
 
FROM {escapeChar}cmsPropertyType{escapeChar}
WHERE (({escapeChar}cmsPropertyType{escapeChar}.{escapeChar}id{escapeChar} = @1))
)) || '|%')".Replace($"\r", string.Empty);
        var typedSql = sql.SQL;
        Assert.That(typedSql, Is.EqualTo(expectedSQL));
    }
}
