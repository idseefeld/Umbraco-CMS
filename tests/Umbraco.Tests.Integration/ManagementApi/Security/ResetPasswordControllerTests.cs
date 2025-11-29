using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_5_0;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Security;

public class ResetPasswordControllerTests : ManagementApiUserGroupTestBase<ResetPasswordController>
{
    private bool IsSmtpConfigured
    {
        get
        {
            var globalSettings = GetRequiredService<IOptions<GlobalSettings>>();
            string? host = globalSettings.Value.Smtp?.Host;
            return !string.IsNullOrEmpty(host);
        }
    }

    protected override Expression<Func<ResetPasswordController, object>> MethodSelector => x => x.RequestPasswordReset(CancellationToken.None, null);

    protected override UserGroupAssertionModel AdminUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = IsSmtpConfigured ? HttpStatusCode.OK : HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel EditorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = IsSmtpConfigured ? HttpStatusCode.OK : HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel SensitiveDataUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = IsSmtpConfigured ? HttpStatusCode.OK : HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel TranslatorUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = IsSmtpConfigured ? HttpStatusCode.OK : HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel WriterUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = IsSmtpConfigured ? HttpStatusCode.OK : HttpStatusCode.BadRequest
    };

    protected override UserGroupAssertionModel UnauthorizedUserGroupAssertionModel => new()
    {
        ExpectedStatusCode = IsSmtpConfigured ? HttpStatusCode.OK : HttpStatusCode.BadRequest
    };

    protected override async Task<HttpResponseMessage> ClientRequest()
    {
        ResetPasswordRequestModel resetPasswordRequestModel = new() { Email = UserEmail };

        return await Client.PostAsync(Url, JsonContent.Create(resetPasswordRequestModel));
    }
}
