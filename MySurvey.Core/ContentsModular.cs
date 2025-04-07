// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi.Abstractions.Modular;
using Biwen.QuickApi.Contents;
using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.Schema;
using MySurvey.Core.Data;

namespace MySurvey.Core;

[PreModular<CoreModular>]
public class ContentsModular : ModularBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddBiwenContents<ApplicationDbContext>(o =>
        {
            //要求需要登录即可访问文档接口,也可以根据自己的逻辑处理
            o.PermissionValidator = async (ctx) =>
            {
                await Task.CompletedTask;

#if DEBUG
                return true;
#else
                return ctx.User.Identity?.IsAuthenticated is true;
#endif
            };
        });

        // Register the content schema generator
        services.AddSingleton<IContentSchemaGenerator, XRenderSchemaGenerator>();
    }
}