// Licensed to the MySurvey.Server under one or more agreements.
// The MySurvey.Server licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySurvey.Core.Entities;
using MySurvey.Core.Interfaces;
using MySurvey.Server.ViewModels;
using System.Security.Claims;

namespace MySurvey.Server.Endpoints;

public static class SurveyApis
{
    /// <summary>
    /// Map Survey Apis
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="apiPrefix"></param>
    /// <returns></returns>
    internal static RouteGroupBuilder MapSurveyApi(this IEndpointRouteBuilder endpoint, string apiPrefix = "api")
    {
        // 创建API组
        var group = endpoint.MapGroup(apiPrefix).RequireAuthorization();

        // 实现问卷调查服务接口

        // 获取问卷列表（需要身份验证）
        group.MapGet("/surveys", [Authorize] async (
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] SurveyStatus status,
            IHttpContextAccessor httpContext,
            ISurveyService surveyService,
            IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var userId = httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var surveys = await surveyService.GetUserSurveysAsync(userId, cancellationToken);

            // 使用Mapster转换为ViewModel
            var result = mapper.Map<IEnumerable<SurveyViewModel>>(surveys);

            return Results.Ok(result);
        });

        // 获取公开问卷列表（允许匿名访问）
        group.MapGet("/public-surveys", async (
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            ISurveyService surveyService,
            IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var surveys = await surveyService.GetSurveysAsync(
                userId: null,
                st: startDate,
                et: endDate,
                status: SurveyStatus.Published,
                cancellationToken: cancellationToken);

            // 使用Mapster转换为ViewModel
            var result = mapper.Map<IEnumerable<PublicSurveyViewModel>>(surveys);

            return Results.Ok(result);
        }).AllowAnonymous();

        // 获取问卷详情（允许匿名访问）
        group.MapGet("/surveys/{id}", async (
            Guid id,
            ISurveyService surveyService,
            IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var survey = await surveyService.GetSurveyAsync(id, cancellationToken);
            if (survey == null)
            {
                return Results.NotFound();
            }

            // 使用Mapster转换为ViewModel
            var surveyDetail = mapper.Map<SurveyDetailViewModel>(survey);

            return Results.Ok(surveyDetail);
        }).AllowAnonymous();

        // 创建问卷（需要身份验证）
        group.MapPost("/surveys", [Authorize] async (
            [FromBody] CreateSurveyRequest request,
            ClaimsPrincipal user,
            ISurveyService surveyService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var survey = new Survey
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                UserId = userId
            };

            var createdSurvey = await surveyService.CreateSurveyAsync(survey, cancellationToken);

            // 返回创建的问卷ID和基本信息
            return Results.Created($"/api/surveys/{createdSurvey.Id}", new { Id = createdSurvey.Id });
        });

        // 更新问卷（需要身份验证）
        group.MapPut("/surveys/{id}", [Authorize] async (
            Guid id,
            [FromBody] UpdateSurveyRequest request,
            ClaimsPrincipal user,
            ISurveyService surveyService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            // 获取问卷并检查所有权
            var existingSurvey = await surveyService.GetSurveyAsync(id, cancellationToken);
            if (existingSurvey == null)
            {
                return Results.NotFound();
            }

            if (existingSurvey.UserId != userId)
            {
                return Results.Forbid();
            }

            existingSurvey.Title = request.Title;
            existingSurvey.Description = request.Description;
            existingSurvey.StartTime = request.StartTime;
            existingSurvey.EndTime = request.EndTime;

            var updatedSurvey = await surveyService.UpdateSurveyAsync(existingSurvey, cancellationToken);

            return Results.Ok(new { Id = updatedSurvey.Id });
        });

        // 删除问卷（需要身份验证）
        group.MapDelete("/surveys/{id}", [Authorize] async (
            Guid id,
            ClaimsPrincipal user,
            ISurveyService surveyService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            // 获取问卷并检查所有权
            var existingSurvey = await surveyService.GetSurveyAsync(id, cancellationToken);
            if (existingSurvey == null)
            {
                return Results.NotFound();
            }

            if (existingSurvey.UserId != userId)
            {
                return Results.Forbid();
            }

            await surveyService.DeleteSurveyAsync(id, cancellationToken);

            return Results.NoContent();
        });

        // 发布问卷（需要身份验证）
        group.MapPost("/surveys/{id}/publish", [Authorize] async (
            Guid id,
            ClaimsPrincipal user,
            ISurveyService surveyService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            // 获取问卷并检查所有权
            var existingSurvey = await surveyService.GetSurveyAsync(id, cancellationToken);
            if (existingSurvey == null)
            {
                return Results.NotFound();
            }

            if (existingSurvey.UserId != userId)
            {
                return Results.Forbid();
            }

            var publishedSurvey = await surveyService.PublishSurveyAsync(id, cancellationToken);

            return Results.Ok(new { Id = publishedSurvey.Id, Status = publishedSurvey.Status });
        });

        // 结束问卷（需要身份验证）
        group.MapPost("/surveys/{id}/end", [Authorize] async (
            Guid id,
            ClaimsPrincipal user,
            ISurveyService surveyService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            // 获取问卷并检查所有权
            var existingSurvey = await surveyService.GetSurveyAsync(id, cancellationToken);
            if (existingSurvey == null)
            {
                return Results.NotFound();
            }

            if (existingSurvey.UserId != userId)
            {
                return Results.Forbid();
            }

            var endedSurvey = await surveyService.EndSurveyAsync(id, cancellationToken);

            return Results.Ok(new { Id = endedSurvey.Id, Status = endedSurvey.Status });
        });

        // 添加问题到问卷（需要身份验证）
        group.MapPost("/surveys/{surveyId}/questions", [Authorize] async (
            Guid surveyId,
            [FromBody] CreateQuestionRequest request,
            ClaimsPrincipal user,
            ISurveyService surveyService,
            IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            // 获取问卷并检查所有权
            var existingSurvey = await surveyService.GetSurveyAsync(surveyId, cancellationToken);
            if (existingSurvey == null)
            {
                return Results.NotFound();
            }

            if (existingSurvey.UserId != userId)
            {
                return Results.Forbid();
            }

            // 使用Mapster转换选项
            var options = mapper.Map<List<SurveyOption>>(request.Options);

            var createdQuestion = await surveyService.AddQuestionAsync(
                surveyId,
                request.Title,
                request.Description,
                request.Type,
                request.IsRequired,
                request.Order,
                options,
                request.ValidationRuleType,  // 添加验证规则类型
                request.CustomValidationPattern,  // 添加自定义验证规则
                request.ValidationErrorMessage,  // 添加验证失败提示信息
                cancellationToken);

            return Results.Created($"/api/surveys/{surveyId}/questions/{createdQuestion.Id}", new { Id = createdQuestion.Id });
        });

        // 更新问题（需要身份验证）
        group.MapPut("/surveys/{surveyId}/questions/{questionId}", [Authorize] async (
            Guid surveyId,
            Guid questionId,
            [FromBody] UpdateQuestionRequest request,
            ClaimsPrincipal user,
            ISurveyService surveyService,
            IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            // 获取问卷并检查所有权
            var existingSurvey = await surveyService.GetSurveyAsync(surveyId, cancellationToken);
            if (existingSurvey == null)
            {
                return Results.NotFound();
            }

            if (existingSurvey.UserId != userId)
            {
                return Results.Forbid();
            }

            // 确保问题属于此问卷
            var question = await surveyService.GetQuestionAsync(questionId, cancellationToken);
            if (question == null || question.SurveyId != surveyId)
            {
                return Results.NotFound();
            }

            // 使用Mapster转换选项
            var options = mapper.Map<List<SurveyOption>>(request.Options);

            var updatedQuestion = await surveyService.UpdateQuestionAsync(
                questionId,
                request.Title,
                request.Description,
                request.Type,
                request.IsRequired,
                request.Order,
                options,
                request.ValidationRuleType,
                request.CustomValidationPattern,
                request.ValidationErrorMessage,
                cancellationToken);

            return Results.Ok(new { Id = updatedQuestion.Id });
        });

        // 删除问题（需要身份验证）
        group.MapDelete("/surveys/{surveyId}/questions/{questionId}", [Authorize] async (
            Guid surveyId,
            Guid questionId,
            ClaimsPrincipal user,
            ISurveyService surveyService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            // 获取问卷并检查所有权
            var existingSurvey = await surveyService.GetSurveyAsync(surveyId, cancellationToken);
            if (existingSurvey == null)
            {
                return Results.NotFound();
            }

            if (existingSurvey.UserId != userId)
            {
                return Results.Forbid();
            }

            // 确保问题属于此问卷
            var question = await surveyService.GetQuestionAsync(questionId, cancellationToken);
            if (question == null || question.SurveyId != surveyId)
            {
                return Results.NotFound();
            }

            await surveyService.DeleteQuestionAsync(questionId, cancellationToken);

            return Results.NoContent();
        });

        // 提交问卷答案（允许匿名访问）
        group.MapPost("/surveys/{surveyId}/submit", async (
            Guid surveyId,
            [FromBody] SubmitSurveyAnswerRequest request,
            ISurveyService surveyService,
            IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // 获取问卷信息
                var survey = await surveyService.GetSurveyAsync(surveyId, cancellationToken);
                if (survey == null)
                {
                    return Results.NotFound();
                }

                if (survey.Status != SurveyStatus.Published)
                {
                    return Results.BadRequest("问卷当前不可提交");
                }

                Guid answerId = Guid.NewGuid();

                // 创建答卷
                var answer = new SurveyAnswer
                {
                    Id = answerId,
                    SurveyId = surveyId,
                    Survey = survey,
                    AnonymousId = request.AnonymousId ?? Guid.NewGuid().ToString(),
                    SubmitTime = DateTime.Now,
                    Status = AnswerStatus.Submitted
                };

                // 使用Mapster转换问题答案
                var questionAnswers = mapper.Map<List<SurveyQuestionAnswer>>(request.QuestionAnswers);

                // 设置答卷ID和修正关联
                foreach (var qa in questionAnswers)
                {
                    qa.AnswerId = answerId;
                    qa.Answer = answer;
                }

                answer.QuestionAnswers = questionAnswers;

                var submittedAnswer = await surveyService.SubmitSurveyAnswerAsync(answer, cancellationToken);

                return Results.Ok(new { AnswerId = submittedAnswer.Id });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }).AllowAnonymous();

        // 获取问卷的答卷列表（需要身份验证）
        group.MapGet("/surveys/{surveyId}/answers", [Authorize] async (
            Guid surveyId,
            ClaimsPrincipal user,
            ISurveyService surveyService,
            IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            // 获取问卷并检查所有权
            var existingSurvey = await surveyService.GetSurveyAsync(surveyId, cancellationToken);
            if (existingSurvey == null)
            {
                return Results.NotFound();
            }

            if (existingSurvey.UserId != userId)
            {
                return Results.Forbid();
            }

            var answers = await surveyService.GetSurveyAnswersAsync(surveyId, cancellationToken);

            // 使用Mapster转换为ViewModel
            var result = mapper.Map<IEnumerable<SurveyAnswerViewModel>>(answers);

            return Results.Ok(result);
        });

        // 获取答卷详情（需要身份验证）
        group.MapGet("/surveys/{surveyId}/answers/{answerId}", [Authorize] async (
            Guid surveyId,
            Guid answerId,
            ClaimsPrincipal user,
            ISurveyService surveyService,
            IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            // 获取问卷并检查所有权
            var existingSurvey = await surveyService.GetSurveyAsync(surveyId, cancellationToken);
            if (existingSurvey == null)
            {
                return Results.NotFound();
            }

            if (existingSurvey.UserId != userId)
            {
                return Results.Forbid();
            }

            var answer = await surveyService.GetSurveyAnswerAsync(answerId, cancellationToken);
            if (answer == null || answer.SurveyId != surveyId)
            {
                return Results.NotFound();
            }

            // 使用Mapster转换为详细的答卷ViewModel
            var answerDetail = mapper.Map<SurveyAnswerDetailViewModel>(answer);

            return Results.Ok(answerDetail);
        });

        // 复制问卷（需要身份验证）
        group.MapPost("/surveys/{id}/copy", [Authorize] async (
            Guid id,
            ClaimsPrincipal user,
            ISurveyService surveyService,
            CancellationToken cancellationToken) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            try
            {
                var copiedSurvey = await surveyService.CopySurveyAsync(id, userId, cancellationToken);
                return Results.Created($"/api/surveys/{copiedSurvey.Id}", new { Id = copiedSurvey.Id });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        //问卷导出使用QuickApi演示!

        //// 导出问卷答卷（需要身份验证）
        //group.MapGet("/surveys/{id}/export", [Authorize] async (
        //    Guid id,
        //    ClaimsPrincipal user,
        //    IExportService exportService,
        //    CancellationToken cancellationToken) =>
        //{
        //    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (userId == null)
        //    {
        //        return Results.Unauthorized();
        //    }

        //    try
        //    {
        //        var stream = await exportService.ExportSurveyAnswersToExcelAsync(id, userId, cancellationToken);
        //        return Results.File(
        //            stream,
        //            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //            $"survey_answers_{id}.xlsx"
        //        );
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return Results.NotFound(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Results.Problem(ex.Message);
        //    }
        //});

        return group;
    }
}
