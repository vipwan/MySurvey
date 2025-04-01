// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi.Infrastructure.Html;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MySurvey.Core.Infrastructure.Interceptors;

/// <summary>
/// XSS清理拦截器，用于检查和清理实体中标记了XssCleanAttribute的属性
/// </summary>
public class XssSanitizationInterceptor(
    IHtmlSanitizerService htmlSanitizerService,
    ILogger<XssSanitizationInterceptor> logger) : SaveChangesInterceptor
{
    private readonly IHtmlSanitizerService _htmlSanitizerService = htmlSanitizerService;
    private readonly ILogger<XssSanitizationInterceptor> _logger = logger;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        SanitizeEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is null)
        {
            return base.SavingChanges(eventData, result);
        }

        SanitizeEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    private void SanitizeEntities(DbContext context)
    {
        // 获取所有被追踪且状态为Added或Modified的实体
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var entityType = entry.Entity.GetType();

            // 查找所有带有XssCleanAttribute的属性
            var properties = entityType.GetProperties()
                .Where(prop =>
                    Attribute.IsDefined(prop, typeof(XssCleanAttribute)) &&
                    prop.PropertyType == typeof(string) &&
                    prop.CanRead &&
                    prop.CanWrite);

            foreach (var property in properties)
            {
                // 获取属性的当前值
                var value = property.GetValue(entry.Entity) as string;

                // 如果值不为空，则进行清理
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        // 清理XSS
                        var sanitizedValue = _htmlSanitizerService.Sanitize(value);

                        // 如果值已更改，则更新实体
                        if (value != sanitizedValue)
                        {
                            property.SetValue(entry.Entity, sanitizedValue);
                            _logger.LogDebug("已清理实体 {EntityType} 的 {PropertyName} 属性中的XSS攻击",
                                entityType.Name, property.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "清理实体 {EntityType} 的 {PropertyName} 属性时出错",
                            entityType.Name, property.Name);
                    }
                }
            }
        }
    }
}
