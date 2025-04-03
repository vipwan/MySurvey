// Licensed to the MySurvey.Server under one or more agreements.
// The MySurvey.Server licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi.Attributes;
using MySurvey.Core.Settings;

namespace MySurvey.Core.Apis;

[QuickApi("/siteSetting")]
[OpenApiMetadata("获取站点设置", "获取站点设置")]
[AuditApi]
public class SiteSettingApi(SiteSetting siteSetting) : BaseQuickApi<EmptyRequest, SiteSetting>
{
    public override ValueTask<SiteSetting> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        return new ValueTask<SiteSetting>(siteSetting);
    }
}
