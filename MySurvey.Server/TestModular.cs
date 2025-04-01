// Licensed to the MySurvey.Server under one or more agreements.
// The MySurvey.Server licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi.Abstractions.Modular;

namespace MySurvey.Server;

public class TestModular : ModularBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
#if DEBUG
        // 跨域配置,如果是开发环境则允许所有跨域请求
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
        });
#endif
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
#if DEBUG
        // 跨域配置
        app.UseCors();
#endif
    }
}