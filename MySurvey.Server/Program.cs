// Licensed to the MySurvey.Server under one or more agreements.
// The MySurvey.Server licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi;
using Biwen.Settings;
using MySurvey.Core.Data;
using MySurvey.Server.Endpoints;
using OfficeOpenXml;

//verison 
Console.WriteLine($"Biwen.QuickApi Version:{Biwen.QuickApi.Generated.Version.AssemblyVersion}");
Console.WriteLine($"Biwen.QuickApi Author:{Biwen.QuickApi.Generated.AssemblyMetadata.Company}");

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var builder = WebApplication.CreateBuilder(args);

// 添加Biwen.Settings
builder.Services.AddBiwenSettings(options =>
{
    options.ProjectId = "MySurvey.Core";

    options.Title = "Biwen.Settings";
    //路由地址 ,http://..../system/settings
    options.Route = "system/settings";
    //授权规则
    options.PermissionValidator = (ctx) => Task.FromResult(ctx.User.Identity?.IsAuthenticated == true);
    options.EditorOptions.EditorOnclick = "return confirm('Are You Sure!?');";
    options.EditorOptions.EdtiorConfirmButtonText = "Submit";
    options.EditorOptions.EditorEditButtonText = "Edit";

    //支持缓存提供者
    options.UseCacheOfMemory();
    //持久层使用EF
    options.UseStoreOfEFCore<ApplicationDbContext>();
});

// 添加Biwen.QuickApis
builder.Services.AddBiwenQuickApis(o => o.RoutePrefix = "api");

var app = builder.Build();

// 确保数据库已创建,仅用于开发环境
// 正式环境中请使用CodeFirst迁移
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.EnsureCreated();

    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "数据库初始化时发生错误");
        throw; // 在开发环境中抛出异常以便查看详细错误信息
    }

    //初始化默认用户
    await IdentitySeed.SeedDefaultUserAsync(scope.ServiceProvider);

    //初始化默认问卷
    await IdentitySeed.SeddDefaultSurveyAsync(scope.ServiceProvider);

}

//use biwen settings
app.UseBiwenSettings();

// Configure the HTTP request pipeline.
//app.UseHttpsRedirection();
//app.MapControllers();

// use Biwen.QuickApis
app.UseBiwenQuickApis();

//map api
app.MapSurveyApi();

app.MapFallbackToFile("/index.html");

app.Run();
