// Licensed to the PersonalFinance.Core under one or more agreements.
// The PersonalFinance.Core 版权归万雅虎&公司内部所有.请勿分发! 
// See the LICENSE file in the project root for more information.
// PersonalFinance.Core SurveyService.cs 
// 2025-03-24 16:23:19  PersonalFinance.Core vipwa 

using Biwen.QuickApi.UnitOfWork;
using Biwen.QuickApi.UnitOfWork.Pagenation;
using Microsoft.EntityFrameworkCore;
using MySurvey.Core.Data;
using MySurvey.Core.Entities;
using MySurvey.Core.Events;
using MySurvey.Core.Interfaces;
using System.Text.RegularExpressions;

namespace MySurvey.Core.Services;

/// <summary>
/// 问卷调查服务
/// </summary>
[AutoInject<ISurveyService>]
public class SurveyService(
    ApplicationDbContext context,
    ILogger<SurveyService> logger) : ISurveyService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<SurveyService> _logger = logger;

    /// <summary>
    /// 创建问卷
    /// </summary>
    public async Task<Survey> CreateSurveyAsync(Survey survey, CancellationToken cancellationToken = default)
    {
        survey.Status = SurveyStatus.Draft;
        survey.CreatedAt = DateTime.Now;

        _context.Surveys.Add(survey);
        await _context.SaveChangesAsync(cancellationToken);

        await new SurveyCreatedEvent(survey).PublishAsync(cancellationToken);
        return survey;
    }

    /// <summary>
    /// 更新问卷
    /// </summary>
    public async Task<Survey> UpdateSurveyAsync(Survey survey, CancellationToken cancellationToken = default)
    {
        var existingSurvey = await _context.Surveys
            .Include(s => s.Questions)
            .FirstOrDefaultAsync(s => s.Id == survey.Id, cancellationToken);

        if (existingSurvey == null)
        {
            throw new KeyNotFoundException($"问卷 {survey.Id} 不存在");
        }

        //if (existingSurvey.Status != SurveyStatus.Draft)
        //{
        //    throw new InvalidOperationException("只能修改草稿状态的问卷");
        //}

        existingSurvey.Title = survey.Title;
        existingSurvey.Description = survey.Description;
        existingSurvey.StartTime = survey.StartTime;
        existingSurvey.EndTime = survey.EndTime;

        existingSurvey.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);

        await new SurveyUpdatedEvent(existingSurvey).PublishAsync(cancellationToken);
        return existingSurvey;
    }

    /// <summary>
    /// 删除问卷
    /// </summary>
    public async Task DeleteSurveyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var survey = await _context.Surveys.FindAsync(id, cancellationToken);
        if (survey == null)
        {
            throw new KeyNotFoundException($"问卷 {id} 不存在");
        }

        if (survey.Status != SurveyStatus.Draft)
        {
            throw new InvalidOperationException("只能删除草稿状态的问卷");
        }

        _context.Surveys.Remove(survey);
        await _context.SaveChangesAsync(cancellationToken);

        await new SurveyDeletedEvent(id).PublishAsync(cancellationToken);
    }

    /// <summary>
    /// 获取问卷详情
    /// </summary>
    public async Task<Survey?> GetSurveyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
            .Include(s => s.Answers)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <summary>
    /// 获取用户的问卷列表，支持分页、状态和时间范围筛选
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="pageNumber">页码，从1开始</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="status">问卷状态，null表示查询所有状态</param>
    /// <param name="startDateFrom">开始时间下限</param>
    /// <param name="startDateTo">开始时间上限</param>
    /// <param name="endDateFrom">结束时间下限</param>
    /// <param name="endDateTo">结束时间上限</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>分页的问卷列表和总记录数</returns>
    public async Task<IPagedList<Survey>> GetUserSurveysAsync(
        string userId,
        int pageNumber = 1,
        int pageSize = 10,
        SurveyStatus? status = null,
        DateTime? startDateFrom = null,
        DateTime? startDateTo = null,
        DateTime? endDateFrom = null,
        DateTime? endDateTo = null,
        CancellationToken cancellationToken = default)
    {
        // 验证分页参数
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // 限制最大页大小

        // 构建基础查询
        var query = _context.Surveys
            .Include(s => s.Questions)
            .Include(s => s.Answers)
            .Where(s => s.UserId == userId);

        // 应用状态筛选
        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        // 应用时间范围筛选
        if (startDateFrom.HasValue)
        {
            query = query.Where(s => s.StartTime >= startDateFrom.Value);
        }

        if (startDateTo.HasValue)
        {
            query = query.Where(s => s.StartTime <= startDateTo.Value);
        }

        if (endDateFrom.HasValue)
        {
            query = query.Where(s => s.EndTime >= endDateFrom.Value);
        }

        if (endDateTo.HasValue)
        {
            query = query.Where(s => s.EndTime <= endDateTo.Value);
        }

        //// 获取总记录数
        //var totalCount = await query.CountAsync(cancellationToken);

        //// 应用排序和分页
        //var surveys = await query
        //    .OrderByDescending(s => s.CreatedAt)
        //    .ThenByDescending(s => s.Status)
        //    .Skip((pageNumber - 1) * pageSize)
        //    .Take(pageSize)
        //    .AsSplitQuery() // 优化性能，拆分查询
        //    .ToListAsync(cancellationToken);

        //排序和拆分查询
        query = query.OrderByDescending(s => s.CreatedAt).ThenByDescending(s => s.Status).AsSplitQuery();

        return await query.ToPagedListAsync(pageNumber - 1, pageSize, cancellationToken: cancellationToken);
    }


    /// <summary>
    /// 发布问卷
    /// </summary>
    public async Task<Survey> PublishSurveyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var survey = await _context.Surveys.FindAsync(id, cancellationToken);
        if (survey == null)
        {
            throw new KeyNotFoundException($"问卷 {id} 不存在");
        }

        //当前都可以发布
        survey.Status = SurveyStatus.Published;

        await _context.SaveChangesAsync(cancellationToken);

        await new SurveyPublishedEvent(survey).PublishAsync(cancellationToken);
        return survey;
    }

    /// <summary>
    /// 结束问卷
    /// </summary>
    public async Task<Survey> EndSurveyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var survey = await _context.Surveys.FindAsync(id, cancellationToken);
        if (survey == null)
        {
            throw new KeyNotFoundException($"问卷 {id} 不存在");
        }

        if (survey.Status != SurveyStatus.Published)
        {
            throw new InvalidOperationException("只能结束已发布的问卷");
        }

        survey.Status = SurveyStatus.Ended;

        await _context.SaveChangesAsync(cancellationToken);

        await new SurveyEndedEvent(survey).PublishAsync(cancellationToken);
        return survey;
    }

    /// <summary>
    /// 提交答卷
    /// </summary>
    public async Task<SurveyAnswer> SubmitSurveyAnswerAsync(SurveyAnswer answer, CancellationToken cancellationToken = default)
    {
        var survey = await _context.Surveys
            .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == answer.SurveyId, cancellationToken);

        if (survey == null)
        {
            throw new KeyNotFoundException($"问卷 {answer.SurveyId} 不存在");
        }

        if (survey.Status != SurveyStatus.Published)
        {
            throw new InvalidOperationException("问卷未发布或已结束");
        }

        if (DateTime.Now > survey.EndTime)
        {
            throw new InvalidOperationException("问卷已过期");
        }

        // 验证文本必填题
        foreach (var question in survey.Questions)
        {
            if (question.IsRequired)
            {
                var questionAnswer = answer.QuestionAnswers.FirstOrDefault(qa => qa.QuestionId == question.Id);
                if (questionAnswer == null)
                {
                    throw new ValidationException($"请回答问题：{question.Title}");
                }
            }
        }

        // 验证必答题和验证规则
        foreach (var question in survey.Questions)
        {
            var questionAnswer = answer.QuestionAnswers.FirstOrDefault(qa => qa.QuestionId == question.Id);
            
            // 验证填空题的验证规则
            if (question.Type == QuestionType.TextInput && questionAnswer != null)
            {
                var textAnswer = questionAnswer.Content;
                if (!string.IsNullOrEmpty(textAnswer))
                {
                    var (isValid, errorMessage) = ValidateAnswer(textAnswer, question.ValidationRuleType, question.CustomValidationPattern);
                    if (!isValid)
                    {
                        throw new ValidationException(question.ValidationErrorMessage ?? errorMessage);
                    }
                }
            }
        }

        // 设置答卷基本信息
        answer.Id = Guid.NewGuid();
        answer.SubmitTime = DateTime.Now;
        answer.Status = AnswerStatus.Submitted;

        // 设置问题答案的ID和关联
        foreach (var questionAnswer in answer.QuestionAnswers)
        {
            questionAnswer.Id = Guid.NewGuid();
            questionAnswer.AnswerId = answer.Id;
            questionAnswer.Answer = answer;

            if(questionAnswer.MatrixAnswers == null)
            {
                continue;
            }
            // 设置矩阵答案的ID和关联
            foreach (var matrixAnswer in questionAnswer.MatrixAnswers)
            {
                matrixAnswer.Id = Guid.NewGuid();
            }
        }

        // 添加答卷及其关联实体
        _context.SurveyAnswers.Add(answer);
        
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await new SurveyAnswerSubmittedEvent(answer).PublishAsync(cancellationToken);
            return answer;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "保存问卷答案时发生数据库错误");
            throw new InvalidOperationException("保存问卷答案失败，请稍后重试");
        }
    }

    private (bool isValid, string errorMessage) ValidateAnswer(string answer, ValidationRuleType? ruleType, string? customPattern)
    {
        if (!ruleType.HasValue)
        {
            return (true, string.Empty);
        }

        return ruleType.Value switch
        {
            ValidationRuleType.PhoneNumber => (Regex.IsMatch(answer, @"^1[3-9]\d{9}$"), "请输入正确的手机号码"),
            ValidationRuleType.IdCard => (Regex.IsMatch(answer, @"(^\d{15}$)|(^\d{18}$)|(^\d{17}(\d|X|x)$)"), "请输入正确的身份证号码"),
            ValidationRuleType.Email => (Regex.IsMatch(answer, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"), "请输入正确的邮箱地址"),
            ValidationRuleType.Custom => ValidateCustomRegex(answer, customPattern),
            _ => (true, string.Empty)
        };
    }

    private static (bool isValid, string errorMessage) ValidateCustomRegex(string answer, string? pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            return (true, string.Empty);
        }

        try
        {
            return (Regex.IsMatch(answer, pattern), "输入格式不正确");
        }
        catch (ArgumentException)
        {
            return (true, string.Empty);
        }
    }

    /// <summary>
    /// 获取问卷的答卷列表
    /// </summary>
    public async Task<IEnumerable<SurveyAnswer>> GetSurveyAnswersAsync(Guid surveyId, CancellationToken cancellationToken = default)
    {
        return await _context.SurveyAnswers
            .Include(a => a.QuestionAnswers)
            .Where(a => a.SurveyId == surveyId)
            .OrderByDescending(a => a.SubmitTime)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 获取答卷详情
    /// </summary>
    public async Task<SurveyAnswer?> GetSurveyAnswerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SurveyAnswers
            .Include(a => a.QuestionAnswers)
                .ThenInclude(qa => qa.Question)
                    .ThenInclude(q => q.Options)
            .Include(a => a.QuestionAnswers)
                .ThenInclude(qa => qa.MatrixAnswers)
                    .ThenInclude(ma => ma.Row)
            .Include(a => a.QuestionAnswers)
                .ThenInclude(qa => qa.MatrixAnswers)
                    .ThenInclude(ma => ma.Column)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    /// <summary>
    /// 更新问卷状态
    /// </summary>
    public async Task<Survey> UpdateSurveyStatusAsync(Guid id, SurveyStatus status, CancellationToken cancellationToken = default)
    {
        var survey = await _context.Surveys.FindAsync(id, cancellationToken);
        if (survey == null)
        {
            throw new KeyNotFoundException($"问卷 {id} 不存在");
        }

        // 验证状态转换的合法性
        if (status == SurveyStatus.Published && survey.Status != SurveyStatus.Draft)
        {
            throw new InvalidOperationException("只能从草稿状态发布问卷");
        }

        if (status == SurveyStatus.Ended && survey.Status != SurveyStatus.Published)
        {
            throw new InvalidOperationException("只能结束已发布的问卷");
        }

        survey.Status = status;
        survey.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);

        // 发布相应的事件
        switch (status)
        {
            case SurveyStatus.Published:
                await new SurveyPublishedEvent(survey).PublishAsync(cancellationToken);
                break;
            case SurveyStatus.Ended:
                await new SurveyEndedEvent(survey).PublishAsync(cancellationToken);
                break;
            case SurveyStatus.Deleted:
                await new SurveyDeletedEvent(id).PublishAsync(cancellationToken);
                break;
        }

        return survey;
    }

    /// <summary>
    /// 获取问题详情
    /// </summary>
    public async Task<SurveyQuestion?> GetQuestionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SurveyQuestions
            .Include(q => q.Survey)
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    /// <summary>
    /// 添加问题
    /// </summary>
    /// <summary>
    /// 添加问题
    /// </summary>
    public async Task<SurveyQuestion> AddQuestionAsync(
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
        CancellationToken cancellationToken = default)
    {
        var survey = await _context.Surveys.FindAsync(surveyId, cancellationToken);
        if (survey == null)
        {
            throw new KeyNotFoundException($"问卷 {surveyId} 不存在");
        }

        if (survey.Status != SurveyStatus.Draft)
        {
            throw new InvalidOperationException("只能修改草稿状态的问卷");
        }

        var question = new SurveyQuestion
        {
            Id = Guid.NewGuid(),
            SurveyId = surveyId,
            Title = title,
            Description = description,
            Type = type,
            IsRequired = isRequired,
            Order = order,
            // 添加验证规则字段
            ValidationRuleType = validationRuleType,
            CustomValidationPattern = customValidationPattern,
            ValidationErrorMessage = validationErrorMessage,
            Survey = survey
        };

        _context.SurveyQuestions.Add(question);
        await _context.SaveChangesAsync(cancellationToken);

        // 添加选项
        foreach (var option in options)
        {
            option.Id = Guid.NewGuid();
            option.QuestionId = question.Id;
            _context.SurveyOptions.Add(option);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await new SurveyQuestionAddedEvent(question).PublishAsync(cancellationToken);
        return question;
    }


    /// <summary>
    /// 更新问题
    /// </summary>
    /// <summary>
    /// 更新问题
    /// </summary>
    public async Task<SurveyQuestion> UpdateQuestionAsync(
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
        CancellationToken cancellationToken = default)
    {
        var question = await _context.SurveyQuestions
            .Include(q => q.Survey)
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (question == null)
        {
            throw new KeyNotFoundException($"问题 {id} 不存在");
        }

        //if (question.Survey.Status != SurveyStatus.Draft)
        //{
        //    throw new InvalidOperationException("只能修改草稿状态的问卷");
        //}

        question.Title = title;
        question.Description = description;
        question.Type = type;
        question.IsRequired = isRequired;
        question.Order = order;

        // 添加验证规则字段的更新
        question.ValidationRuleType = validationRuleType;
        question.CustomValidationPattern = customValidationPattern;
        question.ValidationErrorMessage = validationErrorMessage;

        // 删除旧选项
        _context.SurveyOptions.RemoveRange(question.Options);

        // 添加新选项
        foreach (var option in options)
        {
            option.Id = Guid.NewGuid();
            option.QuestionId = question.Id;
            _context.SurveyOptions.Add(option);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await new SurveyQuestionUpdatedEvent(question).PublishAsync(cancellationToken);
        return question;
    }

    /// <summary>
    /// 删除问题
    /// </summary>
    public async Task DeleteQuestionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var question = await _context.SurveyQuestions
            .Include(q => q.Survey)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

        if (question == null)
        {
            throw new KeyNotFoundException($"问题 {id} 不存在");
        }

        if (question.Survey.Status != SurveyStatus.Draft)
        {
            throw new InvalidOperationException("只能删除草稿状态的问卷");
        }

        _context.SurveyQuestions.Remove(question);
        await _context.SaveChangesAsync(cancellationToken);

        await new SurveyQuestionDeletedEvent(question).PublishAsync(cancellationToken);
    }

    /// <summary>
    /// 提交问卷答案
    /// </summary>
    public async Task<IEnumerable<SurveyAnswer>> SubmitSurveyAnswersAsync(Guid surveyId, IEnumerable<SurveyAnswer> answers, CancellationToken cancellationToken = default)
    {
        var survey = await _context.Surveys.FindAsync(surveyId, cancellationToken);
        if (survey == null)
        {
            throw new KeyNotFoundException($"问卷 {surveyId} 不存在");
        }

        if (survey.Status != SurveyStatus.Published)
        {
            throw new InvalidOperationException("问卷未发布或已结束");
        }

        if (DateTime.Now > survey.EndTime)
        {
            throw new InvalidOperationException("问卷已过期");
        }

        foreach (var answer in answers)
        {
            answer.Id = Guid.NewGuid();
            answer.SurveyId = surveyId;
            answer.SubmitTime = DateTime.Now;
            answer.Status = AnswerStatus.Submitted;
            _context.SurveyAnswers.Add(answer);
        }

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var answer in answers)
        {
            await new SurveyAnswerSubmittedEvent(answer).PublishAsync(cancellationToken);
        }

        return answers;
    }

    /// <summary>
    /// 复制问卷
    /// </summary>
    public async Task<Survey> CopySurveyAsync(Guid surveyId, string userId, CancellationToken cancellationToken = default)
    {
        var sourceSurvey = await _context.Surveys
            .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == surveyId, cancellationToken);

        if (sourceSurvey == null)
        {
            throw new KeyNotFoundException($"问卷 {surveyId} 不存在");
        }

        // 创建新问卷
        var newSurvey = new Survey
        {
            Id = Guid.NewGuid(),
            Title = $"{sourceSurvey.Title} (复制)",
            Description = sourceSurvey.Description,
            Status = SurveyStatus.Draft,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddDays(7),
            UserId = userId,
            CreatedAt = DateTime.Now,
        };

        _context.Surveys.Add(newSurvey);

        // 复制问题
        foreach (var question in sourceSurvey.Questions.OrderBy(q => q.Order))
        {
            var newQuestion = new SurveyQuestion
            {
                Id = Guid.NewGuid(),
                SurveyId = newSurvey.Id,
                Title = question.Title,
                Description = question.Description,
                Type = question.Type,
                IsRequired = question.IsRequired,
                Order = question.Order,
                ValidationRuleType = question.ValidationRuleType,
                CustomValidationPattern = question.CustomValidationPattern,
                ValidationErrorMessage = question.ValidationErrorMessage,
            };

            _context.SurveyQuestions.Add(newQuestion);

            // 复制选项
            foreach (var option in question.Options)
            {
                var newOption = new SurveyOption
                {
                    Id = Guid.NewGuid(),
                    QuestionId = newQuestion.Id,
                    Content = option.Content,
                    Value = option.Value,
                    Order = option.Order,
                    IsDefault = option.IsDefault,
                    IsMatrixRow = option.IsMatrixRow,
                    IsMatrixColumn = option.IsMatrixColumn
                };

                _context.SurveyOptions.Add(newOption);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        await new SurveyCreatedEvent(newSurvey).PublishAsync(cancellationToken);
        return newSurvey;
    }

    public async Task<IEnumerable<Survey>> GetSurveysAsync(string? userId, DateTime? st, DateTime? et, SurveyStatus status = SurveyStatus.Published, CancellationToken cancellationToken = default)
    {
        var query = _context.Surveys.AsQueryable();
        if (userId!=null)
        {
            query = query.Where(s => s.UserId == userId);
        }
        if (st.HasValue)
        {
            query = query.Where(s => s.StartTime >= st);
        }
        if (et.HasValue)
        {
            query = query.Where(s => s.EndTime <= et);
        }

        query = query.Where(s => s.Status == status);

        //获取最近10条数据
        var surveys = await query.OrderByDescending(s => s.CreatedAt).Take(10).ToListAsync(cancellationToken);
        return surveys;
    }

    public async Task<(int SurveyCount, int SurveyCompleteCount, int AnswerCount, int UserCount)> StatAsync(string userId)
    {
        var surveyCount = await _context.Surveys.CountAsync(s => s.UserId == userId);
        var surveyCompleteCount = await _context.Surveys.CountAsync(s => s.UserId == userId && s.Status == SurveyStatus.Ended);

        //查询当前用户问卷的所有答案数量:
        var answerCount = await _context.SurveyAnswers
            .Where(a => _context.Surveys
                .Where(s => s.UserId == userId)
                .Select(s => s.Id)
                .Contains(a.SurveyId))
            .CountAsync();

        // Count distinct users who have submitted answers to the user's surveys
        var userCount = await _context.SurveyAnswers
            .Where(a => _context.Surveys
                .Where(s => s.UserId == userId)
                .Select(s => s.Id)
                .Contains(a.SurveyId))
            .Select(a => a.UserId)
            .Distinct()
            .CountAsync();

        return (surveyCount, surveyCompleteCount, answerCount, userCount);
        
    }
}
