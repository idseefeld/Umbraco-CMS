using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

public class BackOfficeControllerTests : ManagementApiUserGroupTestBase<BackOfficeController>
{
    private string _currentUserEmail = string.Empty;

    protected override Expression<Func<BackOfficeController, object>> MethodSelector =>
        x => x.Login(CancellationToken.None, null);

    protected override string UserEmail => _currentUserEmail;

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = HttpStatusCode.OK
    };

    [Test]
    public override async Task As_Admin_I_Have_Specified_Access()
    {
        _currentUserEmail = "testAdmin@umbraco.com";
        base.As_Admin_I_Have_Specified_Access().GetAwaiter().GetResult();
    }

    [Test]
    public override async Task As_Editor_I_Have_Specified_Access()
    {
        _currentUserEmail = "testEditor@umbraco.com";
        base.As_Editor_I_Have_Specified_Access().GetAwaiter().GetResult();
    }

    [Test]
    public override async Task As_Sensitive_Data_I_Have_Specified_Access()
    {
        _currentUserEmail = "testSensitiveData@umbraco.com";
        base.As_Sensitive_Data_I_Have_Specified_Access().GetAwaiter().GetResult();
    }

    [Test]
    public override async Task As_Translator_I_Have_Specified_Access()
    {
        _currentUserEmail = "testTranslator@umbraco.com";
        base.As_Translator_I_Have_Specified_Access().GetAwaiter().GetResult();
    }

    [Test]
    public override async Task As_Writer_I_Have_Specified_Access()
    {
        _currentUserEmail = "testWriter@umbraco.com";
        base.As_Writer_I_Have_Specified_Access().GetAwaiter().GetResult();
    }

    [Test]
    public override async Task As_Unauthorized_I_Have_Specified_Access()
    {
        _currentUserEmail = "testUnauthorized@invalid.test";
        base.As_Unauthorized_I_Have_Specified_Access().GetAwaiter().GetResult();
    }

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        LoginRequestModel loginRequestModel = new() { Username = _currentUserEmail, Password = UserPassword };
        return await Client.PostAsync(Url, JsonContent.Create(loginRequestModel));
    }

    protected override async Task AuthenticateUser(Guid userGroupKey, string groupName) =>
        await AuthenticateClientAsync(Client, _currentUserEmail, UserPassword, userGroupKey);
}
