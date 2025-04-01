//

namespace MySurvey.Core.Entities;

/// <summary>
/// 问卷答卷
/// </summary>
public class SurveyAnswer
{
    public Guid Id {get;set;}

    /// <summary>
    /// 问卷Id
    /// </summary>
    public required Guid SurveyId { get; set; }

    /// <summary>
    /// 问卷
    /// </summary>
    public virtual required Survey Survey { get; set; }

    /// <summary>
    /// 答卷用户Id,SessionId,只支持匿名
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// 答卷用户（匿名答卷时为null）
    /// </summary>
    //public virtual User? User { get; set; }

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
    public virtual ICollection<SurveyQuestionAnswer> QuestionAnswers { get; set; } = new List<SurveyQuestionAnswer>();
}

/// <summary>
/// 答卷状态
/// </summary>
public enum AnswerStatus
{
    /// <summary>
    /// 进行中
    /// </summary>
    InProgress = 0,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 1,

    /// <summary>
    /// 已提交
    /// </summary>
    Submitted = 2
} 
