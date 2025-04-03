// Licensed to the MySurvey.Server under one or more agreements.
// The MySurvey.Server licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi;
using Biwen.QuickApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.FeatureManagement.Mvc;
using MySurvey.Core.Interfaces;
using System.Security.Claims;

namespace MySurvey.Server.Endpoints;

/// <summary>
/// 使用QuickApi导出问卷答案
/// </summary>
/// <param name="httpContextAccessor"></param>
/// <param name="exportService"></param>
[Authorize]
[QuickApi("/surveys/{id}/export")]
[OpenApiMetadata("导出问卷答案", "导出问卷答案")]

[AuditApi] // 审计API,更多信息请参考:https://github.com/vipwan/Biwen.QuickApi/blob/master/Biwen.QuickApi.DocSite/seed/articles/Auditing.md

[FeatureGate("export")] // 特性开关 演示. 请配置FeatureManagement:export=true,如果是false,则不显示该API抛出404异常

public class ExportApi(IHttpContextAccessor httpContextAccessor,IExportService exportService) : BaseQuickApi
{
    public override async ValueTask<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        // 获取问卷ID
        var id = Guid.Parse(httpContextAccessor.HttpContext.Request.RouteValues["id"]!.ToString()!);

        try
        {
            var stream = await exportService.ExportSurveyAnswersToExcelAsync(id, userId, cancellationToken);
            return Results.File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"survey_answers_{id}.xlsx"
            );
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }
}