// 问卷调查

using Microsoft.AspNetCore.Identity;
using MySurvey.Core.Infrastructure;

namespace MySurvey.Core.Entities;

/// <summary>
/// 问卷调查
/// </summary>
public class Survey
{
    public Guid Id {get;set;}

    /// <summary>
    /// 问卷标题
    /// </summary>
    [XssClean]
    public required string Title { get; set; }

    /// <summary>
    /// 问卷描述
    /// </summary>
    [XssClean]
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
    /// 创建用户Id
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// 创建用户
    /// </summary>
    public virtual IdentityUser? User { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }


    /// <summary>
    /// 问题列表
    /// </summary>
    public virtual ICollection<SurveyQuestion> Questions { get; set; } = [];

    /// <summary>
    /// 答卷列表
    /// </summary>
    public virtual ICollection<SurveyAnswer> Answers { get; set; } = [];
}

/// <summary>
/// 问卷状态
/// </summary>
public enum SurveyStatus
{
    /// <summary>
    /// 草稿
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 已发布
    /// </summary>
    Published = 1,

    /// <summary>
    /// 已结束
    /// </summary>
    Ended = 2,

    /// <summary>
    /// 已删除
    /// </summary>
    Deleted = 3
} 
