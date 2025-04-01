// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi.Abstractions.Modular;
using Biwen.QuickApi.OpenApi;
using Biwen.QuickApi.OpenApi.Scalar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MySurvey.Core.Data;
using MySurvey.Core.Infrastructure.Interceptors;

namespace MySurvey.Core;
public class CoreModular(IConfiguration configuration) : ModularBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        //add http logging
        services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.Request | HttpLoggingFields.RequestQuery;
            options.CombineLogs = true;
        });

        //add services
        services.AddAutoInject();

        //add razor pages
        services.AddRazorPages();

        //add mvc
        services.AddControllersWithViews();

        //policy
        services.Configure<AuthorizationOptions>(options =>
        {
            options.AddPolicy("user", configurePolicy: policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });

        //add authentication
        //services.AddAuthentication(options =>
        //{
        //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //});


        // 注册XSS清理拦截器
        services.AddScoped<XssSanitizationInterceptor>();

        //dbcontext
        // 配置数据库
        services.AddDbContext<ApplicationDbContext>((sp, builder) =>
        {
            // Xss清理拦截器
            var interceptor = sp.GetRequiredService<XssSanitizationInterceptor>();
            // 添加拦截器
            builder.AddInterceptors(interceptor);
            // 配置SQLite
            builder.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
        });

        //identity 
        services.AddIdentityApiEndpoints<IdentityUser>(o =>
        {
            o.User.RequireUniqueEmail = true;
            o.Password.RequiredUniqueChars = 0;
            o.Password.RequireNonAlphanumeric = false;
            o.Password.RequireLowercase = false;
            o.Password.RequireDigit = false;
            o.Password.RequireUppercase = false;

        }).AddEntityFrameworkStores<ApplicationDbContext>();

        //all
        services.AddOpenApi("v1", onlyQuickApi: false);
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        //app.UseHttpsRedirection();

        //添加Keepalive:
        routes.MapGet("/keepalive", () => Results.Content("ok"));

        // api for identity
        routes.MapGroup("account").MapIdentityApi<IdentityUser>();

        app.UseDefaultFiles();
        routes.MapStaticAssets();
        app.UseStaticFiles();

        // razor pages
        routes.MapRazorPages();

        // debug api & logging
        if (((WebApplication)app).Environment.IsDevelopment())
        {
            //Http Logging
            app.UseHttpLogging();
            app.UseDeveloperExceptionPage();

            routes.MapGroup("openapi", app =>
            {
                //swagger ui
                app.MapOpenApi("{documentName}.json");
                app.MapScalarUi();
            });
        }
    }
}