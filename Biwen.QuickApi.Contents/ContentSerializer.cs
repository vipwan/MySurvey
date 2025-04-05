// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-04 21:29:39 ContentSerializer.cs

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.FieldTypes;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Biwen.QuickApi.Contents;

/// <summary>
/// 文档序列化器
/// </summary>
public class ContentSerializer
{
    private readonly ContentFieldManager _fieldManager;
    private readonly JsonSerializerOptions _options;

    public ContentSerializer(ContentFieldManager fieldManager)
    {
        _fieldManager = fieldManager;
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }


    // 缓存所有的Content类型
    private static IEnumerable<IContent>? _contentTypesCache;
    private static readonly Lock _cacheLock = new();

    /// <summary>
    /// 返回所有的文档类型
    /// </summary>
    public IEnumerable<IContent> GetAllContentTypes()
    {
        if (_contentTypesCache != null)
            return _contentTypesCache;

        lock (_cacheLock)
        {
            if (_contentTypesCache != null)
                return _contentTypesCache;

            //使用反射获取所有实现了IContent接口的类型
            var contentTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IContent).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => (IContent)Activator.CreateInstance(t)!)
                .OrderBy(x => x.Content_Order)
                .ToList();

            _contentTypesCache = contentTypes;
            return _contentTypesCache;
        }
    }


    public string SerializeContent<T>(T content) where T : IContent
    {
        var fieldValues = new List<ContentFieldValue>();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            // 只处理IFieldType类型的属性
            if (!typeof(IFieldType).IsAssignableFrom(property.PropertyType))
                continue;

            var fieldInstance = property.GetValue(content) as IFieldType;
            if (fieldInstance == null)
                continue;

            // 获取值字符串表示
            var value = GetFieldStringValue(fieldInstance);

            fieldValues.Add(new ContentFieldValue
            {
                FieldName = property.Name,
                Value = value
            });
        }

        return JsonSerializer.Serialize(fieldValues, _options);
    }

    public T DeserializeContent<T>(string json) where T : IContent, new()
    {
        //{"Title":"耂656","Content":"543543","Description":"54534"}

        var content = new T();
        var fieldValues = JsonSerializer.Deserialize<List<ContentFieldValue>>(json, _options);

        if (fieldValues == null)
            return content;

        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            // 只处理IFieldType类型的属性
            if (!typeof(IFieldType).IsAssignableFrom(property.PropertyType))
                continue;

            var fieldValue = fieldValues.FirstOrDefault(f => f.FieldName == property.Name);
            if (fieldValue == null)
                continue;

            // 创建字段类型实例
            var fieldTypeInstance = CreateFieldTypeInstance(property.PropertyType);
            if (fieldTypeInstance == null)
                continue;

            // 设置字段值
            SetFieldValue(fieldTypeInstance, fieldValue.Value);

            // 设置属性值
            property.SetValue(content, fieldTypeInstance);
        }

        return content;
    }

    // 获取字段的字符串值
    // 添加缓存
    private readonly Dictionary<Type, FieldInfo?> _valueFieldCache = new();
    private readonly Dictionary<Type, PropertyInfo?> _valuePropertyCache = new();

    // 获取字段的字符串值
    private string GetFieldStringValue(IFieldType fieldInstance)
    {
        var type = fieldInstance.GetType();

        // 尝试从缓存获取
        if (!_valueFieldCache.TryGetValue(type, out var valueField))
        {
            valueField = type.GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic);
            _valueFieldCache[type] = valueField;
        }

        if (valueField != null)
        {
            var value = valueField.GetValue(fieldInstance);
            return fieldInstance.ConvertToString(value);
        }

        // 尝试从缓存获取属性
        if (!_valuePropertyCache.TryGetValue(type, out var valueProperty))
        {
            valueProperty = type.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            _valuePropertyCache[type] = valueProperty;
        }

        if (valueProperty != null)
        {
            var value = valueProperty.GetValue(fieldInstance);
            return fieldInstance.ConvertToString(value);
        }

        return string.Empty;
    }
    // 设置字段值
    private void SetFieldValue(IFieldType fieldInstance, string value)
    {
        var convertedValue = fieldInstance.ConvertValue(value);

        // 使用反射设置私有的Value字段
        var valueField = fieldInstance.GetType().GetField("_value",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (valueField != null)
        {
            valueField.SetValue(fieldInstance, convertedValue);
            return;
        }

        // 如果没有_value字段，尝试通过属性设置
        var valueProperty = fieldInstance.GetType().GetProperty("Value",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (valueProperty != null)
        {
            valueProperty.SetValue(fieldInstance, convertedValue);
        }
    }

    // 创建字段类型实例
    private IFieldType? CreateFieldTypeInstance(Type fieldType)
    {
        // 根据字段类型创建实例
        var fieldSystemName = GetFieldTypeSystemName(fieldType);
        if (string.IsNullOrEmpty(fieldSystemName))
            return null;

        var fieldTypeInstance = _fieldManager.GetFieldType(fieldSystemName);

        // 如果是ArrayFieldType，确保初始化Value属性
        if (fieldTypeInstance is ArrayFieldType arrayFieldType && arrayFieldType.Value == null)
        {
            arrayFieldType.Value = string.Empty;
        }

        return fieldTypeInstance;
    }


    // 获取字段类型系统名称
    private string GetFieldTypeSystemName(Type fieldType)
    {
        // 这里需要根据类型获取对应的系统名称
        // 有多种方式，这里假设类型名称和系统名称有简单映射

        if (fieldType == typeof(TextFieldType))
            return "text";
        else if (fieldType == typeof(BooleanFieldType))
            return "boolean";
        else if (fieldType == typeof(IntegerFieldType))
            return "integer";
        else if (fieldType == typeof(NumberFieldType))
            return "number";
        else if (fieldType == typeof(DateTimeFieldType))
            return "datetime";
        else if (fieldType == typeof(TextAreaFieldType))
            return "textArea";
        else if (fieldType == typeof(MarkdownFieldType))
            return "markdown";
        else if (fieldType == typeof(UrlFieldType))
            return "url";
        else if (fieldType == typeof(ColorFieldType))
            return "color";
        else if (fieldType == typeof(ImageFieldType))
            return "image";
        else if (fieldType == typeof(FileFieldType))
            return "file";
        else if (fieldType == typeof(ArrayFieldType))
            return "array";
        else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(ArrayFieldType<>))
            return "array";
        else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(OptionsFieldType<>))
            return "enum";
        else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(OptionsMultiFieldType<>))
            return "enumMulti";

        return string.Empty;
    }
}

public class ContentFieldValue
{
    public string FieldName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}