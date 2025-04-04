﻿// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-04 15:04:37 ServiceRegistration.cs

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.Contents.FieldTypes;
using Biwen.QuickApi.Contents.Schema;
using Biwen.QuickApi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Biwen.QuickApi.Contents;

[SuppressType]
public static class ServiceRegistration
{
    /// <summary>
    /// 注册内容模块
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options">配置</param>
    /// <returns></returns>
    public static IServiceCollection AddBiwenContents<TDbContext>(this IServiceCollection services,
        Action<BiwenContentOptions> options)
        where TDbContext : DbContext, IContentDbContext
    {
        // Options
        services.AddOptions<BiwenContentOptions>().Configure(x => { options?.Invoke(x); });

        // 注册所有字段类型
        services.AddSingleton<IFieldType, TextFieldType>();
        services.AddSingleton<IFieldType, BooleanFieldType>();
        services.AddSingleton<IFieldType, IntegerFieldType>();
        services.AddSingleton<IFieldType, UrlFieldType>();
        services.AddSingleton<IFieldType, ColorFieldType>();
        services.AddSingleton<IFieldType, TextAreaFieldType>();
        services.AddSingleton<IFieldType, MarkdownFieldType>();
        services.AddSingleton<IFieldType, NumberFieldType>();
        services.AddSingleton<IFieldType, DateTimeFieldType>();
        services.AddSingleton<IFieldType, ImageFieldType>();
        services.AddSingleton<IFieldType, FileFieldType>();
        // 注册ArrayFieldType
        services.AddSingleton<IFieldType, ArrayFieldType>();
        //services.AddSingleton<IFieldType, ArrayFieldType<string>>();

        // 注册字段管理器
        services.AddSingleton<ContentFieldManager>();

        //注册Cache
        services.AddMemoryCache();

        // Schema生成器
        services.AddSingleton<IContentSchemaGenerator, XRenderSchemaGenerator>();

        // 注册文件类型提供器
        services.AddSingleton<ContentSerializer>();

        // 注册内容仓储上下文
        services.AddScoped<ContentRepository<TDbContext>>();
        // 注册内容仓储
        services.AddScoped<IContentRepository, ContentRepository<TDbContext>>();


        // 注册GroupRouteBuilder
        services.AddQuickApiGroupRouteBuilder<ContentsGroupRouteBuilder>();

        return services;
    }
}