// Licensed to the MySurvey.Server under one or more agreements.
// The MySurvey.Server licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;
using Biwen.QuickApi;
using MySurvey.Core.Entities;

namespace MySurvey.Server.ViewModels;

#region 问卷相关视图模型

/// <summary>
/// 问卷列表视图模型
/// </summary>
public class SurveyViewModel
{
    /// <summary>
    /// 问卷ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 问卷标题
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// 问卷描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 问卷状态
    /// </summary>
    public SurveyStatus Status { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 问题数量
    /// </summary>
    public int QuestionCount { get; set; }

    /// <summary>
    /// 答卷数量
    /// </summary>
    public int AnswerCount { get; set; }
}

/// <summary>
/// 公开问卷列表视图模型（用于匿名访问）
/// </summary>
public class PublicSurveyViewModel
{
    /// <summary>
    /// 问卷ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 问卷标题
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// 问卷描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; set; }
}

/// <summary>
/// 问卷详情视图模型
/// </summary>
public class SurveyDetailViewModel
{
    /// <summary>
    /// 问卷ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 问卷标题
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// 问卷描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 问卷状态
    /// </summary>
    public SurveyStatus Status { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 问题列表
    /// </summary>
    public List<SurveyQuestionViewModel> Questions { get; set; } = [];
}

/// <summary>
/// 创建问卷请求
/// </summary>
public class CreateSurveyRequest:BaseRequest<CreateSurveyRequest>
{
    /// <summary>
    /// 问卷标题
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 问卷描述
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [Required]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [Required]
    public DateTime EndTime { get; set; }
}

/// <summary>
/// 更新问卷请求
/// </summary>
public class UpdateSurveyRequest:BaseRequest<UpdateSurveyRequest>
{
    /// <summary>
    /// 问卷标题
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 问卷描述
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [Required]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [Required]
    public DateTime EndTime { get; set; }
}

#endregion

#region 问题相关视图模型

/// <summary>
/// 问题视图模型
/// </summary>
public class SurveyQuestionViewModel
{
    /// <summary>
    /// 问题ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 问题标题
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// 问题描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 问题类型
    /// </summary>
    public QuestionType Type { get; set; }

    /// <summary>
    /// 是否必答
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 验证规则类型
    /// </summary>
    public ValidationRuleType? ValidationRuleType { get; set; }

    /// <summary>
    /// 验证失败提示信息
    /// </summary>
    public string? ValidationErrorMessage { get; set; }

    /// <summary>
    /// 选项列表
    /// </summary>
    public List<SurveyOptionViewModel> Options { get; set; } = [];
}

/// <summary>
/// 选项视图模型
/// </summary>
public class SurveyOptionViewModel
{
    /// <summary>
    /// 选项ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 选项内容
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// 选项值
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 是否默认选中
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 是否为矩阵行选项
    /// </summary>
    public bool IsMatrixRow { get; set; }

    /// <summary>
    /// 是否为矩阵列选项
    /// </summary>
    public bool IsMatrixColumn { get; set; }
}

/// <summary>
/// 创建问题请求
/// </summary>
public class CreateQuestionRequest:BaseRequest<CreateQuestionRequest>
{
    /// <summary>
    /// 问题标题
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 问题描述
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// 问题类型
    /// </summary>
    [Required]
    public QuestionType Type { get; set; }

    /// <summary>
    /// 是否必答
    /// </summary>
    [Required]
    public bool IsRequired { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Required]
    public int Order { get; set; }

    /// <summary>
    /// 验证规则类型
    /// </summary>
    public ValidationRuleType? ValidationRuleType { get; set; }

    /// <summary>
    /// 自定义验证规则正则表达式
    /// </summary>
    public string? CustomValidationPattern { get; set; }

    /// <summary>
    /// 验证失败提示信息
    /// </summary>
    public string? ValidationErrorMessage { get; set; }

    /// <summary>
    /// 选项列表
    /// </summary>
    public List<CreateOptionRequest> Options { get; set; } = [];
}

/// <summary>
/// 创建选项请求
/// </summary>
public class CreateOptionRequest:BaseRequest<CreateOptionRequest>
{
    /// <summary>
    /// 选项内容
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Content { get; set; } = null!;

    /// <summary>
    /// 选项值
    /// </summary>
    [StringLength(100)]
    public string? Value { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Required]
    public int Order { get; set; }

    /// <summary>
    /// 是否默认选中
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 是否为矩阵行选项
    /// </summary>
    public bool IsMatrixRow { get; set; }

    /// <summary>
    /// 是否为矩阵列选项
    /// </summary>
    public bool IsMatrixColumn { get; set; }
}

/// <summary>
/// 更新问题请求
/// </summary>
public class UpdateQuestionRequest:BaseRequest<UpdateQuestionRequest>
{
    /// <summary>
    /// 问题标题
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 问题描述
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// 问题类型
    /// </summary>
    [Required]
    public QuestionType Type { get; set; }

    /// <summary>
    /// 是否必答
    /// </summary>
    [Required]
    public bool IsRequired { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Required]
    public int Order { get; set; }

    /// <summary>
    /// 验证规则类型
    /// </summary>
    public ValidationRuleType? ValidationRuleType { get; set; }

    /// <summary>
    /// 自定义验证规则正则表达式
    /// </summary>
    public string? CustomValidationPattern { get; set; }

    /// <summary>
    /// 验证失败提示信息
    /// </summary>
    public string? ValidationErrorMessage { get; set; }

    /// <summary>
    /// 选项列表
    /// </summary>
    public List<CreateOptionRequest> Options { get; set; } = [];
}

#endregion

#region 答卷相关视图模型

/// <summary>
/// 答卷列表视图模型
/// </summary>
public class SurveyAnswerViewModel
{
    /// <summary>
    /// 答卷ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 问卷ID
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// 匿名答卷标识符
    /// </summary>
    public string? AnonymousId { get; set; }

    /// <summary>
    /// 提交时间
    /// </summary>
    public DateTime SubmitTime { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompleteTime { get; set; }

    /// <summary>
    /// 答卷状态
    /// </summary>
    public AnswerStatus Status { get; set; }

    /// <summary>
    /// 回答问题数量
    /// </summary>
    public int QuestionCount { get; set; }
}

/// <summary>
/// 答卷详情视图模型
/// </summary>
public class SurveyAnswerDetailViewModel
{
    /// <summary>
    /// 答卷ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 问卷ID
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// 匿名答卷标识符
    /// </summary>
    public string? AnonymousId { get; set; }

    /// <summary>
    /// 提交时间
    /// </summary>
    public DateTime SubmitTime { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompleteTime { get; set; }

    /// <summary>
    /// 答卷状态
    /// </summary>
    public AnswerStatus Status { get; set; }

    /// <summary>
    /// 问题答案列表
    /// </summary>
    public List<QuestionAnswerViewModel> QuestionAnswers { get; set; } = [];
}

/// <summary>
/// 问题答案视图模型
/// </summary>
public class QuestionAnswerViewModel
{
    /// <summary>
    /// 答案ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 问题ID
    /// </summary>
    public Guid QuestionId { get; set; }

    /// <summary>
    /// 问题标题
    /// </summary>
    public string? QuestionTitle { get; set; }

    /// <summary>
    /// 答案内容（填空题）
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 选项ID列表（单选/多选题）
    /// </summary>
    public List<Guid> OptionIds { get; set; } = [];

    /// <summary>
    /// 选项值列表（单选/多选题）
    /// </summary>
    public List<string> OptionValues { get; set; } = [];

    /// <summary>
    /// 评分值（评分题）
    /// </summary>
    public int? RatingValue { get; set; }

    /// <summary>
    /// 矩阵答案（矩阵题）
    /// </summary>
    public List<MatrixAnswerViewModel> MatrixAnswers { get; set; } = [];
}

/// <summary>
/// 矩阵答案视图模型
/// </summary>
public class MatrixAnswerViewModel
{
    /// <summary>
    /// 行ID
    /// </summary>
    public Guid RowId { get; set; }

    /// <summary>
    /// 行内容
    /// </summary>
    public string? RowContent { get; set; }

    /// <summary>
    /// 列ID
    /// </summary>
    public Guid ColumnId { get; set; }

    /// <summary>
    /// 列内容
    /// </summary>
    public string? ColumnContent { get; set; }

    /// <summary>
    /// 答案值
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// 提交问卷答案请求
/// </summary>
public class SubmitSurveyAnswerRequest:BaseRequest<SubmitSurveyAnswerRequest>
{
    /// <summary>
    /// 匿名答卷标识符（可选）
    /// </summary>
    public string? AnonymousId { get; set; }

    /// <summary>
    /// 问题答案列表
    /// </summary>
    [Required]
    public List<SubmitQuestionAnswerRequest> QuestionAnswers { get; set; } = [];
}

/// <summary>
/// 提交问题答案请求
/// </summary>
public class SubmitQuestionAnswerRequest: BaseRequest<SubmitQuestionAnswerRequest>
{
    /// <summary>
    /// 问题ID
    /// </summary>
    [Required]
    public Guid QuestionId { get; set; }

    /// <summary>
    /// 答案内容（填空题）
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 选项ID列表（单选/多选题）
    /// </summary>
    public List<Guid>? OptionIds { get; set; }

    /// <summary>
    /// 选项值列表（单选/多选题）
    /// </summary>
    public List<string>? OptionValues { get; set; }

    /// <summary>
    /// 评分值（评分题）
    /// </summary>
    public int? RatingValue { get; set; }

    /// <summary>
    /// 矩阵答案（矩阵题）
    /// </summary>
    public List<SubmitMatrixAnswerRequest>? MatrixAnswers { get; set; }
}

/// <summary>
/// 提交矩阵答案请求
/// </summary>
public class SubmitMatrixAnswerRequest:BaseRequest<SubmitMatrixAnswerRequest>
{
    /// <summary>
    /// 行ID
    /// </summary>
    [Required]
    public Guid RowId { get; set; }

    /// <summary>
    /// 列ID
    /// </summary>
    [Required]
    public Guid ColumnId { get; set; }

    /// <summary>
    /// 答案值
    /// </summary>
    [Required]
    public string Value { get; set; } = string.Empty;
}

#endregion
