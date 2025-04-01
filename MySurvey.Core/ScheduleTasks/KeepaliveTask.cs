// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi.Scheduling;
using MySurvey.Core.Settings;

namespace MySurvey.Core.ScheduleTasks;

/// <summary>
/// 保活任务，每15分钟执行一次，确保系统持续运行
/// </summary>
[ScheduleTask("0/15 * * * *", Description = "keep alive", IsAsync = true)]
public class KeepaliveTask(ILogger<KeepaliveTask> logger, IServiceScopeFactory serviceScopeFactory) : OnlyOneRunningScheduleTask
{
    /// <summary>
    /// 执行保活任务
    /// </summary>
    /// <returns>表示异步操作的任务</returns>
    public override async Task ExecuteAsync()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var siteSetting = scope.ServiceProvider.GetRequiredService<SiteSetting>();

        // 如果没有配置Host，则不执行保活任务
        if (siteSetting.Host == null)
            return;

        try
        {
            using var client = new HttpClient()
            {
                // 设置超时时间10秒
                Timeout = TimeSpan.FromSeconds(10)
            };

            var url = $"{siteSetting.Host.TrimEnd('/')}/keepalive";
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("KeepaliveTask is running");
            }
            else
            {
                logger.LogError("KeepaliveTask is failed");
            }
        }
        catch
        {
            logger.LogError("KeepaliveTask is failed");
        }
    }
}
