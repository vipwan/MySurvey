// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi.Infrastructure.StartupTask;

namespace MySurvey.Core.ScheduleTasks;

/// <summary>
/// 启动程序执行的操作演示
/// </summary>
/// <param name="logger"></param>
public class CopyrightTask(ILogger<CopyrightTask> logger) : StartupTaskBase
{
    public override Task ExecuteAsync(CancellationToken ct)
    {
        //verison 
        logger.LogInformation("Biwen.QuickApi Version:{AssemblyVersion}", Biwen.QuickApi.Generated.Version.AssemblyVersion);
        logger.LogInformation("Biwen.QuickApi Author:{Company}", Biwen.QuickApi.Generated.AssemblyMetadata.Company);

        return Task.CompletedTask;
    }
}
