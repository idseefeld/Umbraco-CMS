using System.Diagnostics;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.PostgreSqlSecurity;

public sealed class DocumentTypeContentScenario
{
    public const string CreateDocumentType = "CreateDocumentType";
    public const string UpdateDocumentType = "UpdateDocumentType";
    public const string CreateContent = "CreateContent";
    public const string UpdateContent = "UpdateContent";
    public const string VerifyReadBack = "VerifyReadBack";

    private readonly IContentTypeService _contentTypeService;
    private readonly IContentEditingService _contentEditingService;
    private readonly Action<StepRecord> _sink;

    public DocumentTypeContentScenario(
        IContentTypeService contentTypeService,
        IContentEditingService contentEditingService,
        Action<StepRecord> sink)
    {
        _contentTypeService = contentTypeService;
        _contentEditingService = contentEditingService;
        _sink = sink;
    }

    public async Task RunIterationAsync(int worker, int iteration)
    {
        var suffix = $"w{worker}i{iteration}{Guid.NewGuid():N}"[..20];
        var alias = $"pgSec{suffix}";
        var name = $"PgSec {suffix}";
        var updatedName = name + " updated";
        var contentName = $"Page {suffix}";
        var updatedContentName = contentName + " updated";
        const string createdTitle = "created";
        const string updatedTitle = "updated";

        IContentType? contentType = null;
        Guid contentKey = Guid.Empty;

        var ok = await MeasureAsync(worker, iteration, CreateDocumentType, async () =>
        {
            contentType = new ContentTypeBuilder()
                .WithAlias(alias)
                .WithName(name)
                .AddPropertyGroup()
                    .WithAlias("content")
                    .WithName("Content")
                    .WithSupportsPublishing(true)
                    .AddPropertyType()
                        .WithAlias("title")
                        .WithName("Title")
                        .Done()
                    .AddPropertyType()
                        .WithAlias("body")
                        .WithName("Body")
                        .Done()
                    .Done()
                .Build();
            contentType.AllowedAsRoot = true;
            RequireSuccess(await _contentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey));
        });

        ok = ok && await MeasureAsync(worker, iteration, UpdateDocumentType, async () =>
        {
            contentType!.Name = updatedName;
            contentType.Description = "updated by security test";
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Description = "updated";
            }

            RequireSuccess(await _contentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey));
        });

        ok = ok && await MeasureAsync(worker, iteration, CreateContent, async () =>
        {
            var result = await _contentEditingService.CreateAsync(
                new ContentCreateModel
                {
                    ContentTypeKey = contentType!.Key,
                    Variants = [new () { Name = contentName }],
                    Properties =
                    [
                        new () { Alias = "title", Value = createdTitle },
                        new () { Alias = "body", Value = "body " + suffix },
                    ],
                },
                Constants.Security.SuperUserKey);
            RequireSuccess(result.Success, result.Status);
            contentKey = result.Result.Content!.Key;
        });

        ok = ok && await MeasureAsync(worker, iteration, UpdateContent, async () =>
        {
            var result = await _contentEditingService.UpdateAsync(
                contentKey,
                new ContentUpdateModel
                {
                    Variants = [new () { Name = updatedContentName }],
                    Properties = [new () { Alias = "title", Value = updatedTitle }],
                },
                Constants.Security.SuperUserKey);
            RequireSuccess(result.Success, result.Status);
        });

        if (ok)
        {
            await MeasureAsync(worker, iteration, VerifyReadBack, async () =>
            {
                var type = await _contentTypeService.GetAsync(contentType!.Key)
                    ?? throw new InvalidOperationException("Document type not found after update.");
                if (type.Name != updatedName)
                {
                    throw new InvalidOperationException($"Document type name mismatch: '{type.Name}'.");
                }

                var content = await _contentEditingService.GetAsync(contentKey)
                    ?? throw new InvalidOperationException("Content not found after update.");
                if (content.Name != updatedContentName)
                {
                    throw new InvalidOperationException($"Content name mismatch: '{content.Name}'.");
                }

                if (content.GetValue<string>("title") != updatedTitle)
                {
                    throw new InvalidOperationException("Content property value mismatch after update.");
                }
            });
        }
    }

    private static void RequireSuccess(Attempt<ContentTypeOperationStatus> attempt) =>
        RequireSuccess(attempt.Success, attempt.Result);

    private static void RequireSuccess(bool success, object? status)
    {
        if (!success)
        {
            throw new InvalidOperationException($"Operation failed with status {status}.");
        }
    }

    private async Task<bool> MeasureAsync(int worker, int iteration, string step, Func<Task> action)
    {
        var started = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await action();
            stopwatch.Stop();
            _sink(new StepRecord(worker, iteration, step, started, stopwatch.Elapsed.TotalMilliseconds, true, null, null));
            return true;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _sink(new StepRecord(worker, iteration, step, started, stopwatch.Elapsed.TotalMilliseconds, false, ex.GetType().Name, ex.Message));
            return false;
        }
    }
}
