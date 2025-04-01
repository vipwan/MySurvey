// Licensed to the PersonalFinance.Core under one or more agreements.
// The PersonalFinance.Core 版权归万雅虎&公司内部所有.请勿分发! 
// See the LICENSE file in the project root for more information.
// PersonalFinance.Core SurveyEvents.cs 
// 2025-03-24 16:22:47  PersonalFinance.Core vipwa 


using MySurvey.Core.Entities;

namespace MySurvey.Core.Events;

/// <summary>
/// 问卷创建事件
/// </summary>
public record SurveyCreatedEvent(Survey Survey) : IEvent;

/// <summary>
/// 问卷更新事件
/// </summary>
public record SurveyUpdatedEvent(Survey Survey) : IEvent;

/// <summary>
/// 问卷删除事件
/// </summary>
public record SurveyDeletedEvent(Guid SurveyId) : IEvent;

/// <summary>
/// 问卷发布事件
/// </summary>
public record SurveyPublishedEvent(Survey Survey) : IEvent;

/// <summary>
/// 问卷结束事件
/// </summary>
public record SurveyEndedEvent(Survey Survey) : IEvent;

/// <summary>
/// 问卷答卷提交事件
/// </summary>
public record SurveyAnswerSubmittedEvent(SurveyAnswer Answer) : IEvent; 
