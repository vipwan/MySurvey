// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan

using Biwen.QuickApi.Attributes;
using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.Contents.Services;
using Microsoft.AspNetCore.Http;

namespace Biwen.QuickApi.Contents.Apis;

/// <summary>
/// 获取内容版本列表的API
/// </summary>
[QuickApi("/{id:guid}/versions", Group = Constants.GroupName)]
[OpenApiMetadata("获取内容版本列表", "获取指定内容的所有版本")]
public class GetContentVersionsApi(IContentVersionService versionService, IHttpContextAccessor httpContextAccessor) : BaseQuickApi<EmptyRequest, IEnumerable<ContentVersion>>
{
    public override async ValueTask<IEnumerable<ContentVersion>> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        var id = httpContextAccessor.HttpContext!.Request.RouteValues["id"]?.ToString();
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var contentId))
        {
            throw new ArgumentException("无效的ID");
        }

        return await versionService.GetVersionsAsync(contentId);
    }
}

/// <summary>
/// 获取指定版本内容的API
/// </summary>
[QuickApi("/{id:guid}/versions/{version:int}", Group = Constants.GroupName)]
[OpenApiMetadata("获取指定版本内容", "获取指定内容的指定版本")]
public class GetContentVersionApi(IContentVersionService versionService, IHttpContextAccessor httpContextAccessor) : BaseQuickApi<EmptyRequest, ContentVersion?>
{
    public override async ValueTask<ContentVersion?> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        var id = httpContextAccessor.HttpContext!.Request.RouteValues["id"]?.ToString();
        var version = httpContextAccessor.HttpContext!.Request.RouteValues["version"]?.ToString();

        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var contentId))
        {
            throw new ArgumentException("无效的ID");
        }

        if (string.IsNullOrEmpty(version) || !int.TryParse(version, out var versionNumber))
        {
            throw new ArgumentException("无效的版本号");
        }

        return await versionService.GetVersionAsync(contentId, versionNumber);
    }
}