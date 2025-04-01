// Licensed to the MySurvey.Server under one or more agreements.
// The MySurvey.Server licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi;
using Biwen.QuickApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using MySurvey.Core.Interfaces;
using System.Security.Claims;

namespace MySurvey.Server.Endpoints;

public record StatData(int SurveyCount,int SurveyCompleteCount,int AnswerCount,int UserCount);

[Authorize]
[QuickApi("/dashboard-data")]
[OpenApiMetadata("面板统计", "面板统计数据")]
public class DashboardApi(ISurveyService surveyService, IHttpContextAccessor httpContextAccessor) : BaseQuickApi
{
    public override async ValueTask<IResult> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var (surveyCount, surveyCompleteCount, answerCount, userCount) = await surveyService.StatAsync(userId!);

        return Results.Json(
            new StatData(
            surveyCount,
            surveyCompleteCount,
            answerCount,
            userCount));
    }
}