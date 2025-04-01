// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MySurvey.Core.Settings;

/// <summary>
/// 站点设置类，用于存储和管理网站的基本配置信息
/// </summary>
/// <remarks>
/// 该类继承自ValidationSettingBase，支持自动验证功能
/// 包含网站名称、描述、Logo等基础信息
/// </remarks>
[Description("站点设置")]
public class SiteSetting : ValidationSettingBase<SiteSetting>
{
    /// <summary>
    /// 获取或设置站点名称
    /// </summary>
    /// <value>默认值为"个人财务管理"</value>
    [Required, Description("站点名称")]
    public string SiteName { get; set; } = "My调查";

    /// <summary>
    /// 获取或设置站点描述信息
    /// </summary>
    /// <value>默认值为"PersonalFinance 个人财务管理系统"</value>
    [Required, StringLength(200, MinimumLength = 2), Description("站点描述")]
    public string SiteDescription { get; set; } = "我的调查 - MySurvey,Biwen.QuickApi示例项目";

    /// <summary>
    /// 获取或设置站点Logo的URL路径
    /// </summary>
    /// <value>默认值为"/images/logo.png"</value>
    [Description("站点Logo")]
    public string SiteLogo { get; set; } = "/images/logo.png";

    /// <summary>
    /// 获取或设置网站备案信息
    /// </summary>
    /// <value>默认值为"粤ICP备11106044号"</value>
    [Description("备案信息")]
    public string ICP { get; set; } = "粤ICP备XXXXXX号";

    /// <summary>
    /// 获取或设置客服电话
    /// </summary>
    /// <value>默认值为"400-0400755"</value>
    [Description("客服电话")]
    public string CustomerServicePhone { get; set; } = "400-XXXXXX";

    /// <summary>
    /// 获取或设置网站访问地址
    /// </summary>
    /// <value>默认值为"http://localhost:5289"</value>
    /// <remarks>
    /// 用于构建完整的URL地址，如系统内部访问、通知邮件中的链接等
    /// 在生产环境中应配置为实际的域名
    /// </remarks>
    [Required, Url, StringLength(200)]
    [Description("Host地址,如:http://localhost:5289")]
    public string Host { get; set; } = "http://localhost:5289";
}
