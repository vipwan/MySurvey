// Licensed to the PersonalFinance.Core under one or more agreements.
// The PersonalFinance.Core 版权归万雅虎&公司内部所有.请勿分发! 
// See the LICENSE file in the project root for more information.
// PersonalFinance.Core SurveyQuestionEventHandler.cs 

using MySurvey.Core.Events;

namespace MySurvey.Core.EventHandlers;

public class SurveyQuestionEventHandler(ILogger<SurveyQuestionEventHandler> logger):
    IEventSubscriber<SurveyQuestionAddedEvent>,
    IEventSubscriber<SurveyQuestionUpdatedEvent>,
    IEventSubscriber<SurveyQuestionDeletedEvent>
{
    public Task HandleAsync(SurveyQuestionAddedEvent @event, CancellationToken ct)
    {
        logger.LogDebug("问卷问题添加事件处理" + @event.Question.Id);
        // 问卷问题添加事件处理
        return Task.CompletedTask;
    }
    public  Task HandleAsync(SurveyQuestionUpdatedEvent @event, CancellationToken ct)
    {
        logger.LogDebug("问卷问题更新事件处理" + @event.Question.Id);
        // 问卷问题更新事件处理
        return Task.CompletedTask;
    }
    public  Task HandleAsync(SurveyQuestionDeletedEvent @event, CancellationToken ct)
    {
        logger.LogDebug("问卷问题删除事件处理" + @event.Question.Id);
        // 问卷问题删除事件处理
        return Task.CompletedTask;
    }
}
