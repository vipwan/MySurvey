// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MySurvey.Core.Infrastructure;

/// <summary>
/// 标记字段过滤Xss攻击
/// </summary>

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal class XssCleanAttribute : Attribute
{
}
