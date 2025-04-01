// Licensed to the PersonalFinance.Core under one or more agreements.
// The PersonalFinance.Core 版权归万雅虎&公司内部所有.请勿分发! 
// See the LICENSE file in the project root for more information.
// PersonalFinance.Core SurveyQuestionEvents.cs 
// 2025-03-24 21:49:39  PersonalFinance.Core vipwa 

using MySurvey.Core.Entities;

namespace MySurvey.Core.Events;

/// <summary>
/// 问卷问题添加事件
/// </summary>
public record SurveyQuestionAddedEvent(SurveyQuestion Question) : IEvent;

/// <summary>
/// 问卷问题更新事件
/// </summary>
public record SurveyQuestionUpdatedEvent(SurveyQuestion Question) : IEvent;


/// <summary>
/// 问卷问题删除事件
/// </summary>
public record SurveyQuestionDeletedEvent(SurveyQuestion Question) : IEvent;
