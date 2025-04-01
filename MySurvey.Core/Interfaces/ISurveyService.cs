// Licensed to the PersonalFinance.Core under one or more agreements.
// The PersonalFinance.Core 版权归万雅虎&公司内部所有.请勿分发! 
// See the LICENSE file in the project root for more information.
// PersonalFinance.Core ISurveyService.cs 

using MySurvey.Core.Entities;

namespace MySurvey.Core.Interfaces;

/// <summary>
/// 问卷调查服务接口
/// </summary>
public interface ISurveyService
{
    /// <summary>
    /// 创建问卷
    /// </summary>
    Task<Survey> CreateSurveyAsync(Survey survey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新问卷
    /// </summary>
    Task<Survey> UpdateSurveyAsync(Survey survey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除问卷
    /// </summary>
    Task DeleteSurveyAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取问卷详情
    /// </summary>
    Task<Survey?> GetSurveyAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的问卷列表
    /// </summary>
    Task<IEnumerable<Survey>> GetUserSurveysAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发布问卷
    /// </summary>
    Task<Survey> PublishSurveyAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 结束问卷
    /// </summary>
    Task<Survey> EndSurveyAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新问卷状态
    /// </summary>
    Task<Survey> UpdateSurveyStatusAsync(Guid id, SurveyStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// 提交答卷
    /// </summary>
    Task<SurveyAnswer> SubmitSurveyAnswerAsync(SurveyAnswer answer, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取问题详情
    /// </summary>
    Task<SurveyQuestion?> GetQuestionAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加问题
    /// </summary>
    /// <summary>
    /// 添加问题
    /// </summary>
    Task<SurveyQuestion> AddQuestionAsync(
        Guid surveyId,
        string title,
        string? description,
        QuestionType type,
        bool isRequired,
        int order,
        IEnumerable<SurveyOption> options,
        ValidationRuleType? validationRuleType = null,
        string? customValidationPattern = null,
        string? validationErrorMessage = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新问题
    /// </summary>
    /// <summary>
    /// 更新问题
    /// </summary>
    Task<SurveyQuestion> UpdateQuestionAsync(
        Guid id,
        string title,
        string? description,
        QuestionType type,
        bool isRequired,
        int order,
        IEnumerable<SurveyOption> options,
        ValidationRuleType? validationRuleType = null,
        string? customValidationPattern = null,
        string? validationErrorMessage = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除问题
    /// </summary>
    Task DeleteQuestionAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取问卷的答卷列表
    /// </summary>
    Task<IEnumerable<SurveyAnswer>> GetSurveyAnswersAsync(Guid surveyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取答卷详情
    /// </summary>
    Task<SurveyAnswer?> GetSurveyAnswerAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 提交问卷答案
    /// </summary>
    /// <param name="surveyId">问卷ID</param>
    /// <param name="answers">答案列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>提交的答案列表</returns>
    Task<IEnumerable<SurveyAnswer>> SubmitSurveyAnswersAsync(Guid surveyId, IEnumerable<SurveyAnswer> answers, CancellationToken cancellationToken = default);

    /// <summary>
    /// 复制问卷
    /// </summary>
    /// <param name="surveyId">原问卷ID</param>
    /// <param name="userId">新问卷创建者ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新创建的问卷</returns>
    Task<Survey> CopySurveyAsync(Guid surveyId, string userId, CancellationToken cancellationToken = default);


    /// <summary>
    /// 查询最近10条问卷列表
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="st"></param>
    /// <param name="et"></param>
    /// <param name="status"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<Survey>> GetSurveysAsync(string? userId, DateTime? st, DateTime? et, SurveyStatus status = SurveyStatus.Published, CancellationToken cancellationToken = default);


    /// <summary>
    /// 统计
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<(int SurveyCount, int SurveyCompleteCount, int AnswerCount, int UserCount)> StatAsync(string userId);

}
