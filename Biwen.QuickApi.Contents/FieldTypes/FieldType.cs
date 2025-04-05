// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-04 15:03:59 TextFieldType.cs

using Biwen.QuickApi.Contents.Abstractions;

namespace Biwen.QuickApi.Contents.FieldTypes;

/// <summary>
/// 文本字段实现
/// </summary>
public class TextFieldType : IFieldType
{
    public string Name => "文本";
    public string SystemName => "text";
    public Type ValueType => typeof(string);

    public object? ConvertValue(string value) => value;
    public string ConvertToString(object? value) => value?.ToString() ?? string.Empty;
    public bool Validate(string value, string? rules = null) => true;

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => null;
}

/// <summary>
/// URL字段实现
/// </summary>
public class UrlFieldType : IFieldType
{
    public string Name => "URL";
    public string SystemName => "url";
    public Type ValueType => typeof(string);

    public object? ConvertValue(string value) => value;
    public string ConvertToString(object? value) => value?.ToString() ?? string.Empty;
    public bool Validate(string value, string? rules = null)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => "请输入有效的URL";
}

/// <summary>
/// 颜色字段实现
/// </summary>
public class ColorFieldType : IFieldType
{
    public string Name => "颜色";
    public string SystemName => "color";
    public Type ValueType => typeof(string);

    public object? ConvertValue(string value) => value;
    public string ConvertToString(object? value) => value?.ToString() ?? string.Empty;
    public bool Validate(string value, string? rules = null)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(value, "^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");
    }

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => "请输入有效的颜色值（如 #FF0000）";
}

/// <summary>
/// TextArea字段实现
/// </summary>
public class TextAreaFieldType : IFieldType
{
    public string Name => "TextArea";
    public string SystemName => "textArea";
    public Type ValueType => typeof(string);

    public object? ConvertValue(string value) => value;
    public string ConvertToString(object? value) => value?.ToString() ?? string.Empty;
    public bool Validate(string value, string? rules = null) => true;

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => null;
}

/// <summary>
/// MarkDown字段实现
/// </summary>
public class MarkdownFieldType : IFieldType
{
    public string Name => "Markdown";
    public string SystemName => "markdown";
    public Type ValueType => typeof(string);
    public object? ConvertValue(string value) => value;
    public string ConvertToString(object? value) => value?.ToString() ?? string.Empty;
    public bool Validate(string value, string? rules = null) => true;

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => null;
}

/// <summary>
/// 日期时间字段实现
/// </summary>
public class DateTimeFieldType : IFieldType
{
    public string Name => "日期时间";
    public string SystemName => "datetime";
    public Type ValueType => typeof(DateTime);

    public object? ConvertValue(string value)
    {
        if (DateTime.TryParse(value, out var result))
        {
            return result;
        }
        return null;
    }

    public string ConvertToString(object? value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        return string.Empty;
    }

    public bool Validate(string value, string? rules = null)
    {
        return DateTime.TryParse(value, out _);
    }

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => "请输入有效的日期时间";
}

/// <summary>
/// 整数字段实现
/// </summary>
public class IntegerFieldType : IFieldType
{
    public string Name => "整数";
    public string SystemName => "integer";
    public Type ValueType => typeof(int);

    public object? ConvertValue(string value)
    {
        if (int.TryParse(value, out var result))
        {
            return result;
        }
        return null;
    }

    public string ConvertToString(object? value)
    {
        if (value is int intValue)
        {
            return intValue.ToString();
        }
        return string.Empty;
    }

    public bool Validate(string value, string? rules = null)
    {
        return int.TryParse(value, out _);
    }

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => "请输入有效的整数";
}

/// <summary>
/// 布尔字段实现
/// </summary>
public class BooleanFieldType : IFieldType
{
    public string Name => "布尔值";
    public string SystemName => "boolean";
    public Type ValueType => typeof(bool);

    public object? ConvertValue(string value)
    {
        if (bool.TryParse(value, out var result))
        {
            return result;
        }
        return null;
    }

    public string ConvertToString(object? value)
    {
        if (value is bool boolValue)
        {
            return boolValue.ToString();
        }
        return string.Empty;
    }

    public bool Validate(string value, string? rules = null)
    {
        return bool.TryParse(value, out _);
    }

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => "请输入有效的布尔值";
}

/// <summary>
/// 数字字段实现
/// </summary>
public class NumberFieldType : IFieldType
{
    public string Name => "数字";
    public string SystemName => "number";
    public Type ValueType => typeof(double);

    public object? ConvertValue(string value)
    {
        if (double.TryParse(value, out var result))
        {
            return result;
        }
        return null;
    }

    public string ConvertToString(object? value)
    {
        if (value is double doubleValue)
        {
            return doubleValue.ToString();
        }
        return string.Empty;
    }

    public bool Validate(string value, string? rules = null)
    {
        return double.TryParse(value, out _);
    }

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => "请输入有效的数字";
}

/// <summary>
/// 图片字段实现
/// </summary>
public class ImageFieldType : IFieldType
{
    public string Name => "图片";
    public string SystemName => "image";
    public Type ValueType => typeof(string);

    public object? ConvertValue(string value) => value;
    public string ConvertToString(object? value) => value?.ToString() ?? string.Empty;
    public bool Validate(string value, string? rules = null) => true;

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => null;
}

/// <summary>
/// 文件字段实现
/// </summary>
public class FileFieldType : IFieldType
{
    public string Name => "文件";
    public string SystemName => "file";
    public Type ValueType => typeof(string);

    public object? ConvertValue(string value) => value;
    public string ConvertToString(object? value) => value?.ToString() ?? string.Empty;
    public bool Validate(string value, string? rules = null) => true;

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => null;
}

/// <summary>
/// 数组字段实现（泛型）
/// </summary>
/// <typeparam name="T">数组元素类型</typeparam>
public class ArrayFieldType<T> : IFieldType
{
    public string Name => $"数组<{typeof(T).Name}>";
    public string SystemName => $"array<{typeof(T).Name.ToLower()}>";
    public Type ValueType => typeof(T[]);

    public object? ConvertValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Array.Empty<T>();
        }

        var items = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var result = new List<T>();

        foreach (var item in items)
        {
            try
            {
                if (typeof(T) == typeof(string))
                {
                    result.Add((T)(object)item);
                }
                else if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(item, out var intValue))
                    {
                        result.Add((T)(object)intValue);
                    }
                }
                else if (typeof(T) == typeof(double))
                {
                    if (double.TryParse(item, out var doubleValue))
                    {
                        result.Add((T)(object)doubleValue);
                    }
                }
                else if (typeof(T) == typeof(bool))
                {
                    if (bool.TryParse(item, out var boolValue))
                    {
                        result.Add((T)(object)boolValue);
                    }
                }
                else if (typeof(T) == typeof(DateTime))
                {
                    if (DateTime.TryParse(item, out var dateTimeValue))
                    {
                        result.Add((T)(object)dateTimeValue);
                    }
                }
            }
            catch
            {
                // 忽略转换错误
            }
        }

        return result.ToArray();
    }

    public string ConvertToString(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is Array array)
        {
            return string.Join(",", array.Cast<object>().Select(o => o.ToString()));
        }

        return value.ToString() ?? string.Empty;
    }

    public bool Validate(string value, string? rules = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        var items = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var item in items)
        {
            try
            {
                if (typeof(T) == typeof(string))
                {
                    // 字符串类型不需要验证
                }
                else if (typeof(T) == typeof(int))
                {
                    if (!int.TryParse(item, out _))
                    {
                        return false;
                    }
                }
                else if (typeof(T) == typeof(double))
                {
                    if (!double.TryParse(item, out _))
                    {
                        return false;
                    }
                }
                else if (typeof(T) == typeof(bool))
                {
                    if (!bool.TryParse(item, out _))
                    {
                        return false;
                    }
                }
                else if (typeof(T) == typeof(DateTime))
                {
                    if (!DateTime.TryParse(item, out _))
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        return true;
    }

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => $"请输入有效的{typeof(T).Name}数组，多个值用逗号分隔";
}

/// <summary>
/// 数组字段实现（字符串数组）
/// </summary>
public class ArrayFieldType : IFieldType
{
    public string Name => "字符串数组";
    public string SystemName => "array<string>";
    public Type ValueType => typeof(string[]);

    public object? ConvertValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Array.Empty<string>();
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries);
    }

    public string ConvertToString(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is string[] array)
        {
            return string.Join(",", array);
        }

        return value.ToString() ?? string.Empty;
    }

    public bool Validate(string value, string? rules = null)
    {
        // 字符串数组不需要特殊验证
        return true;
    }

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => null;
}

/// <summary>
/// 枚举单选字段实现
/// </summary>
/// <typeparam name="T">枚举类型</typeparam>
public class OptionsFieldType<T> : IFieldType where T : Enum
{
    public string Name => $"枚举单选<{typeof(T).Name}>";
    public string SystemName => $"options<{typeof(T).Name.ToLower()}>";
    public Type ValueType => typeof(T);

    public object? ConvertValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (int.TryParse(value, out var intValue))
        {
            return Enum.ToObject(typeof(T), intValue);
        }

        return null;
    }

    public string ConvertToString(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is T enumValue)
        {
            return Convert.ToInt32(enumValue).ToString();
        }

        return value.ToString() ?? string.Empty;
    }

    public bool Validate(string value, string? rules = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        if (int.TryParse(value, out var intValue))
        {
            return Enum.IsDefined(typeof(T), intValue);
        }

        return false;
    }

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => $"请输入有效的{typeof(T).Name}枚举值";
}

/// <summary>
/// 枚举多选字段实现
/// </summary>
/// <typeparam name="T">枚举类型</typeparam>
public class OptionsMultiFieldType<T> : IFieldType where T : Enum
{
    public string Name => $"枚举多选<{typeof(T).Name}>";
    public string SystemName => $"options-multi<{typeof(T).Name.ToLower()}>";
    public Type ValueType => typeof(T[]);

    public object? ConvertValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Array.Empty<T>();
        }

        var items = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var result = new List<T>();

        foreach (var item in items)
        {
            if (int.TryParse(item, out var intValue) && Enum.IsDefined(typeof(T), intValue))
            {
                result.Add((T)Enum.ToObject(typeof(T), intValue));
            }
        }

        return result.ToArray();
    }

    public string ConvertToString(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        if (value is Array array)
        {
            return string.Join(",", array.Cast<object>().Select(o => Convert.ToInt32(o).ToString()));
        }

        return value.ToString() ?? string.Empty;
    }

    public bool Validate(string value, string? rules = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        var items = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var item in items)
        {
            if (!int.TryParse(item, out var intValue) || !Enum.IsDefined(typeof(T), intValue))
            {
                return false;
            }
        }

        return true;
    }

    private object? _value;

    /// <summary>
    /// 值属性
    /// </summary>
    public string Value
    {
        get => _value?.ToString() ?? string.Empty;
        set => _value = value;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    /// <returns>字段值</returns>
    public object? GetValue() => _value;

    /// <summary>
    /// 设置字段值
    /// </summary>
    /// <param name="value">字段值</param>
    public void SetValue(object? value) => _value = value;

    /// <summary>
    /// 获取验证错误消息
    /// </summary>
    /// <returns>验证错误消息</returns>
    public string? GetValidationErrorMessage() => $"请输入有效的{typeof(T).Name}枚举值，多个值用逗号分隔";
}
