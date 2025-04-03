// Licensed to the MySurvey.Server under one or more agreements.
// The MySurvey.Server licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi;
using Biwen.QuickApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using MySurvey.Core.Interfaces;
using System.Security.Claims;

namespace MySurvey.Server.Endpoints;

/// <summary>
/// 返回的统计数据
/// </summary>
/// <param name="SurveyCount"></param>
/// <param name="SurveyCompleteCount"></param>
/// <param name="AnswerCount"></param>
/// <param name="UserCount"></param>
public record StatData(int SurveyCount,int SurveyCompleteCount,int AnswerCount,int UserCount);

[Authorize]
[QuickApi("/dashboard-data")]
[OpenApiMetadata("面板统计", "面板统计数据")]
[AuditApi] // 审计API
public class DashboardApi(ISurveyService surveyService, IHttpContextAccessor httpContextAccessor) : BaseQuickApi<EmptyRequest, StatData>
{
    public override async ValueTask<StatData> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (surveyCount, surveyCompleteCount, answerCount, userCount) = await surveyService.StatAsync(userId!);

        return new StatData(
            surveyCount,
            surveyCompleteCount,
            answerCount,
            userCount);
    }
}