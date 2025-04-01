// Licensed to the PersonalFinance.Core under one or more agreements.
// The PersonalFinance.Core 版权归万雅虎&公司内部所有.请勿分发! 
// See the LICENSE file in the project root for more information.
// PersonalFinance.Core SurveyQuestionAnswer.cs 
// 2025-03-24 16:29:33  PersonalFinance.Core vipwa 

using MySurvey.Core.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySurvey.Core.Entities;

/// <summary>
/// 问卷问题答案
/// </summary>
public class SurveyQuestionAnswer
{
    public Guid Id {get;set;}


    /// <summary>
    /// 答卷Id
    /// </summary>
    public required Guid AnswerId { get; set; }

    /// <summary>
    /// 答卷
    /// </summary>
    public virtual required SurveyAnswer Answer { get; set; }

    /// <summary>
    /// 问题Id
    /// </summary>
    public required Guid QuestionId { get; set; }

    /// <summary>
    /// 问题
    /// </summary>
    public virtual required SurveyQuestion Question { get; set; }

    /// <summary>
    /// 答案内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 选项Id列表
    /// </summary>
    public virtual ICollection<Guid> OptionIds { get; set; } = [];

    /// <summary>
    /// 选项值列表
    /// </summary>
    public virtual ICollection<string> OptionValues { get; set; } = [];

    /// <summary>
    /// 评分值
    /// </summary>
    public int? RatingValue { get; set; }

    /// <summary>
    /// 矩阵答案
    /// </summary>
    public virtual ICollection<MatrixAnswer> MatrixAnswers { get; set; } = [];
}

/// <summary>
/// 矩阵答案
/// </summary>
[Table("SurveyMatrixAnswer")]
public class MatrixAnswer
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 行Id
    /// </summary>
    public required Guid RowId { get; set; }

    /// <summary>
    /// 行选项
    /// </summary>
    public virtual required SurveyOption Row { get; set; }

    /// <summary>
    /// 列Id
    /// </summary>
    public required Guid ColumnId { get; set; }

    /// <summary>
    /// 列选项
    /// </summary>
    public virtual required SurveyOption Column { get; set; }

    /// <summary>
    /// 答案值
    /// </summary>
    [XssClean]
    public required string Value { get; set; }
} 
