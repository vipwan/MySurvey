// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MySurvey.Core.Events;

namespace MySurvey.Core.EventHandlers;

/// <summary>
/// 当用户提交问卷时的事件处理程序
/// </summary>
/// <param name="logger"></param>
public class SurveyAnswerSubmittedEventHandler(ILogger<SurveyAnswerSubmittedEventHandler> logger) : IEventSubscriber<SurveyAnswerSubmittedEvent>
{
    public Task HandleAsync(SurveyAnswerSubmittedEvent @event, CancellationToken ct)
    {
        logger.LogInformation("用户提交了问卷: {SurveyId}", @event.Answer.SurveyId);
        return Task.CompletedTask;
    }
}