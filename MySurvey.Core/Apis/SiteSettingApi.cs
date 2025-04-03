// Licensed to the MySurvey.Server under one or more agreements.
// The MySurvey.Server licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi.Attributes;
using MySurvey.Core.Settings;

namespace MySurvey.Core.Apis;

/// <summary>
/// 返回站点设置的DTO类,这里使用了AutoDto特性来自动生成DTO
/// </summary>
[AutoDto<SiteSetting>(nameof(SiteSetting.Order))]
public partial class SiteSettingDto { }

[QuickApi("/siteSetting")]
[OpenApiMetadata("获取站点设置", "获取站点设置")]
[AuditApi]
public class SiteSettingApi(SiteSetting siteSetting) : BaseQuickApi<EmptyRequest, SiteSettingDto>
{
    public override ValueTask<SiteSettingDto> ExecuteAsync(EmptyRequest request, CancellationToken cancellationToken = default)
    {
        //这里演示通过AutoDto生成的扩展方法转化DTO,而不是通过Mapper,不用反射属性提升性能!
        var dto = siteSetting.MapperToSiteSettingDto();
        return new ValueTask<SiteSettingDto>(dto);
    }
}
