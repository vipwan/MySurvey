// Licensed to the PersonalFinance.Core under one or more agreements.
// The PersonalFinance.Core 版权归万雅虎&公司内部所有.请勿分发! 
// See the LICENSE file in the project root for more information.
// PersonalFinance.Core SurveyQuestion.cs 
// 2025-03-24 16:29:23  PersonalFinance.Core vipwa 

using MySurvey.Core.Infrastructure;

namespace MySurvey.Core.Entities;

/// <summary>
/// 问卷问题
/// </summary>
public class SurveyQuestion
{
    public Guid Id { get; set; }


    /// <summary>
    /// 问卷Id
    /// </summary>
    public required Guid SurveyId { get; set; }

    /// <summary>
    /// 问卷
    /// </summary>
    public virtual Survey Survey { get; set; } = null!;

    /// <summary>
    /// 问题标题
    /// </summary>
    [XssClean]
    public required string Title { get; set; }

    /// <summary>
    /// 问题描述
    /// </summary>
    [XssClean]
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
    public virtual ICollection<SurveyOption> Options { get; set; } = [];

    /// <summary>
    /// 答案列表
    /// </summary>
    public virtual ICollection<SurveyQuestionAnswer> Answers { get; set; } = [];
}

/// <summary>
/// 验证规则类型
/// </summary>
public enum ValidationRuleType
{
    /// <summary>
    /// 无验证
    /// </summary>
    [Description("无验证")]
    None = 0,

    /// <summary>
    /// 手机号码
    /// </summary>
    [Description("手机号码")]
    PhoneNumber = 1,

    /// <summary>
    /// 身份证号码
    /// </summary>
    [Description("身份证号码")]
    IdCard = 2,

    /// <summary>
    /// 电子邮件
    /// </summary>
    [Description("电子邮件")]
    Email = 3,

    /// <summary>
    /// 自定义正则表达式
    /// </summary>
    [Description("自定义正则表达式")]
    Custom = 4
}

/// <summary>
/// 问题类型
/// </summary>
[AutoDescription]
public enum QuestionType
{
    /// <summary>
    /// 单选题
    /// </summary>
    [Description("单选题")]
    SingleChoice = 0,

    /// <summary>
    /// 多选题
    /// </summary>
    [Description("多选题")]
    MultipleChoice = 1,

    /// <summary>
    /// 填空题
    /// </summary>
    [Description("填空题")]
    TextInput = 2,

    /// <summary>
    /// 评分题
    /// </summary>
    [Description("评分题")]
    Rating = 3,

    /// <summary>
    /// 矩阵单选题
    /// </summary>
    [Description("矩阵单选题")]
    MatrixSingleChoice = 4,

    /// <summary>
    /// 矩阵多选题
    /// </summary>
    [Description("矩阵多选题")]
    MatrixMultipleChoice = 5
}
