// Licensed to the PersonalFinance.Core under one or more agreements.
// The PersonalFinance.Core 版权归万雅虎&公司内部所有.请勿分发! 
// See the LICENSE file in the project root for more information.
// PersonalFinance.Core SurveyOption.cs 
// 2025-03-24 16:29:13  PersonalFinance.Core vipwa 

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySurvey.Core.Entities;

/// <summary>
/// 问卷选项
/// </summary>
public class SurveyOption
{
    /// <summary>
    /// 选项ID
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// 问题ID
    /// </summary>
    public Guid QuestionId { get; set; }

    /// <summary>
    /// 选项内容
    /// </summary>
    [Required]
    [StringLength(500)]
    public required string Content { get; set; }

    /// <summary>
    /// 选项值
    /// </summary>
    [StringLength(100)]
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

    /// <summary>
    /// 关联的问题
    /// </summary>
    [ForeignKey("QuestionId")]
    public virtual SurveyQuestion? Question { get; set; }
} 
