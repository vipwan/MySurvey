// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-04 17:11:56 XRenderSchemaGenerator.cs

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.FieldTypes;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Biwen.QuickApi.Contents.Schema;

/// <summary>
/// XRender Schema生成服务，生成符合FormRender 2.0表单引擎规范的Schema
/// </summary>
public class XRenderSchemaGenerator : IContentSchemaGenerator
{
    private readonly IMemoryCache _memoryCache;
    private const string CACHE_KEY_PREFIX = "XRenderSchema_";

    public XRenderSchemaGenerator(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public virtual JsonObject GenerateSchema<T>() where T : IContent
    {
        return GenerateSchema(typeof(T));
    }

    public virtual JsonObject GenerateSchema(Type contentType)
    {
        if (!typeof(IContent).IsAssignableFrom(contentType))
        {
            throw new ArgumentException($"类型 {contentType.Name} 必须实现 IContent 接口");
        }

        // 尝试从缓存获取
        string cacheKey = $"{CACHE_KEY_PREFIX}{contentType.FullName}";
        if (_memoryCache.TryGetValue(cacheKey, out JsonObject? cachedSchema) && cachedSchema != null)
        {
            return cachedSchema;
        }

        // XRender Schema 根节点
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject()
        };

        // 定义需要的字段
        var required = new JsonArray();

        var properties = contentType.GetProperties();
        foreach (var property in properties)
        {
            if (!typeof(IFieldType).IsAssignableFrom(property.PropertyType))
                continue;

            var propertySchema = GeneratePropertySchema(property);
            if (propertySchema != null)
            {
                ((JsonObject)schema["properties"]!)[property.Name] = propertySchema;

                // 如果是必填字段，添加到required数组中
                var requiredAttr = property.GetCustomAttribute<RequiredAttribute>();
                if (requiredAttr != null)
                {
                    required.Add(property.Name);
                }
            }
        }

        // 如果有必填字段，添加到schema中
        if (required.Count > 0)
        {
            schema["required"] = required;
        }

        // 将结果存入缓存
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromHours(24)) // 设置滑动过期时间为24小时
            .SetAbsoluteExpiration(TimeSpan.FromDays(7)); // 设置绝对过期时间为7天

        _memoryCache.Set(cacheKey, schema, cacheOptions);

        return schema;
    }

    public virtual string GenerateSchemaJson<T>(JsonSerializerOptions? options = null) where T : IContent
    {
        return GenerateSchemaJson(typeof(T), options);
    }

    public virtual string GenerateSchemaJson(Type contentType, JsonSerializerOptions? options = null)
    {
        var schema = GenerateSchema(contentType);
        return JsonSerializer.Serialize(schema, options ?? new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private JsonObject? GeneratePropertySchema(PropertyInfo property)
    {
        var propertyType = property.PropertyType;
        var schema = new JsonObject();

        // 设置标题
        schema["title"] = property.Name;
        schema["x-decorator"] = "FormItem";

        // 获取显示名称特性
        var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
        if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name))
        {
            schema["title"] = displayAttr.Name;
        }
        else
        {
            // 如果是枚举类型,使用DescriptionAttribute,如果没有使用名称
            if (propertyType.IsEnum)
            {
                var enumName = propertyType.Name;
                var enumField = propertyType.GetField(enumName);
                if (enumField != null)
                {
                    var descAttr = enumField.GetCustomAttribute<DescriptionAttribute>();
                    if (descAttr != null && !string.IsNullOrEmpty(descAttr.Description))
                    {
                        schema["title"] = descAttr.Description;
                    }
                }
            }
        }

        // 获取描述特性
        var descriptionAttr = property.GetCustomAttribute<DescriptionAttribute>();
        if (descriptionAttr != null && !string.IsNullOrEmpty(descriptionAttr.Description))
        {
            schema["description"] = descriptionAttr.Description;
        }

        // 处理文本字段类型
        if (propertyType == typeof(TextFieldType))
        {
            schema["type"] = "string";
            schema["widget"] = "input";
            schema["x-component"] = "Input";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(string));

            var props = new JsonObject
            {
                ["placeholder"] = $"请输入{schema["title"]}"
            };

            schema["props"] = new JsonObject
            {
                ["placeholder"] = $"请输入{schema["title"]}"
            };

            // 处理字符串长度限制
            var stringLengthAttr = property.GetCustomAttribute<StringLengthAttribute>();
            if (stringLengthAttr != null)
            {
                if (stringLengthAttr.MinimumLength > 0)
                {
                    schema["minLength"] = stringLengthAttr.MinimumLength;
                    schema["min"] = stringLengthAttr.MinimumLength;
                }
                schema["maxLength"] = stringLengthAttr.MaximumLength;
                schema["max"] = stringLengthAttr.MaximumLength;
                props["maxLength"] = stringLengthAttr.MaximumLength;
            }

            // 处理最小长度限制
            var minLengthAttr = property.GetCustomAttribute<MinLengthAttribute>();
            if (minLengthAttr != null && stringLengthAttr == null)
            {
                schema["minLength"] = minLengthAttr.Length;
                schema["min"] = minLengthAttr.Length;
            }

            // 处理正则表达式验证
            var regexAttr = property.GetCustomAttribute<RegularExpressionAttribute>();
            if (regexAttr != null)
            {
                schema["pattern"] = regexAttr.Pattern;
                schema["x-validator"] = "pattern";

                // 添加错误消息对象
                if (schema["message"] == null)
                {
                    schema["message"] = new JsonObject();
                }
                if (!string.IsNullOrEmpty(regexAttr.ErrorMessage))
                {
                    ((JsonObject)schema["message"]!)["pattern"] = regexAttr.ErrorMessage;
                }
                else
                {
                    ((JsonObject)schema["message"]!)["pattern"] = "请输入正确的格式";
                }
            }

            // 处理数据类型验证
            var dataTypeAttr = property.GetCustomAttribute<DataTypeAttribute>();
            if (dataTypeAttr != null)
            {
                switch (dataTypeAttr.DataType)
                {
                    case DataType.EmailAddress:
                        schema["format"] = "email";
                        schema["x-validator"] = "email";
                        props["placeholder"] = "请输入邮箱地址";
                        ((JsonObject)schema["props"]!)["placeholder"] = "请输入邮箱地址";
                        break;
                    case DataType.PhoneNumber:
                        schema["format"] = "tel";
                        schema["x-validator"] = "phone";
                        props["placeholder"] = "请输入电话号码";
                        ((JsonObject)schema["props"]!)["placeholder"] = "请输入电话号码";
                        break;
                    case DataType.Url:
                        schema["format"] = "url";
                        schema["x-validator"] = "url";
                        props["placeholder"] = "请输入URL";
                        ((JsonObject)schema["props"]!)["placeholder"] = "请输入URL";
                        break;
                    case DataType.Password:
                        schema["widget"] = "password";
                        schema["x-component"] = "Password";
                        break;
                    case DataType.MultilineText:
                        schema["widget"] = "textarea";
                        schema["x-component"] = "TextArea";
                        break;
                }
            }

            // 处理电子邮件特性
            var emailAttr = property.GetCustomAttribute<EmailAddressAttribute>();
            if (emailAttr != null)
            {
                schema["format"] = "email";
                schema["x-validator"] = "email";
                props["placeholder"] = "请输入邮箱地址";
                ((JsonObject)schema["props"]!)["placeholder"] = "请输入邮箱地址";
            }

            // 处理电话号码特性
            var phoneAttr = property.GetCustomAttribute<PhoneAttribute>();
            if (phoneAttr != null)
            {
                schema["format"] = "tel";
                schema["x-validator"] = "phone";
                props["placeholder"] = "请输入电话号码";
                ((JsonObject)schema["props"]!)["placeholder"] = "请输入电话号码";

                // 添加错误消息
                if (schema["message"] == null)
                {
                    schema["message"] = new JsonObject();
                }
                ((JsonObject)schema["message"]!)["pattern"] = "Invalid phone number format.";
            }

            // 添加组件属性
            if (props.Count > 0)
            {
                schema["x-component-props"] = props;
            }
        }
        // 处理URL字段类型
        else if (propertyType == typeof(UrlFieldType))
        {
            schema["type"] = "string";
            schema["format"] = "url";
            schema["widget"] = "input";
            schema["x-component"] = "Input";
            schema["x-validator"] = "url";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(string));

            var props = new JsonObject
            {
                ["placeholder"] = "请输入URL"
            };
            schema["x-component-props"] = props;
            schema["props"] = new JsonObject
            {
                ["placeholder"] = "请输入URL"
            };
        }
        // 处理颜色字段类型
        else if (propertyType == typeof(ColorFieldType))
        {
            schema["type"] = "string";
            schema["format"] = "color";
            schema["widget"] = "color";
            schema["x-component"] = "ColorPicker";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(string));
        }
        // 处理文本区域字段类型
        else if (propertyType == typeof(TextAreaFieldType))
        {
            schema["type"] = "string";
            schema["widget"] = "textArea";
            schema["x-component"] = "TextArea";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(string));

            var props = new JsonObject();
            schema["props"] = new JsonObject();

            // 处理字符串长度限制
            var stringLengthAttr = property.GetCustomAttribute<StringLengthAttribute>();
            if (stringLengthAttr != null)
            {
                if (stringLengthAttr.MinimumLength > 0)
                {
                    schema["minLength"] = stringLengthAttr.MinimumLength;
                    schema["min"] = stringLengthAttr.MinimumLength;
                }
                schema["maxLength"] = stringLengthAttr.MaximumLength;
                schema["max"] = stringLengthAttr.MaximumLength;
                props["maxLength"] = stringLengthAttr.MaximumLength;
            }

            // 处理最小长度限制
            var minLengthAttr = property.GetCustomAttribute<MinLengthAttribute>();
            if (minLengthAttr != null && stringLengthAttr == null)
            {
                schema["minLength"] = minLengthAttr.Length;
                schema["min"] = minLengthAttr.Length;
            }

            // 添加组件属性
            if (props.Count > 0)
            {
                schema["x-component-props"] = props;
            }
        }
        // 处理Markdown字段类型
        else if (propertyType == typeof(MarkdownFieldType))
        {
            schema["type"] = "string";
            schema["widget"] = "markdown";
            schema["x-component"] = "Markdown";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(string));
        }
        // 处理日期时间字段类型
        else if (propertyType == typeof(DateTimeFieldType))
        {
            schema["type"] = "string";
            schema["widget"] = "datePicker";
            schema["x-component"] = "DatePicker";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(DateTime));

            //showTime根据DisplayFormatAttribute来判断,包含HH:mm则为true,否则false
            var showTime = false;
            var displayFormatAttr = property.GetCustomAttribute<DisplayFormatAttribute>();
            if (displayFormatAttr != null && !string.IsNullOrEmpty(displayFormatAttr.DataFormatString))
            {
                // 判断是否包含HH:mm
                showTime = displayFormatAttr.DataFormatString.Contains("HH:mm");
            }

            var props = new JsonObject
            {
                ["showTime"] = showTime
            };

            schema["x-component-props"] = props;
            schema["props"] = new JsonObject
            {
                ["showTime"] = showTime
            };
        }
        // 处理整数字段类型
        else if (propertyType == typeof(IntegerFieldType))
        {
            schema["type"] = "number";
            schema["widget"] = "inputNumber";
            schema["x-component"] = "NumberPicker";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(int));

            var props = new JsonObject
            {
                ["precision"] = 0
            };

            // 处理Range特性
            var rangeAttr = property.GetCustomAttribute<RangeAttribute>();
            if (rangeAttr != null)
            {
                // 设置最小值和最大值 - FormRender 2.0 格式
                schema["minimum"] = Convert.ToDouble(rangeAttr.Minimum);
                schema["maximum"] = Convert.ToDouble(rangeAttr.Maximum);
                schema["min"] = Convert.ToDouble(rangeAttr.Minimum);
                schema["max"] = Convert.ToDouble(rangeAttr.Maximum);

                // 组件属性中的min和max
                props["min"] = Convert.ToDouble(rangeAttr.Minimum);
                props["max"] = Convert.ToDouble(rangeAttr.Maximum);
            }

            schema["x-component-props"] = props;

            // 添加错误消息
            schema["message"] = new JsonObject
            {
                ["required"] = ""
            };
        }
        // 处理布尔字段类型
        else if (propertyType == typeof(BooleanFieldType))
        {
            schema["type"] = "boolean";
            schema["widget"] = "switch";
            schema["x-component"] = "Switch";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(bool));
        }
        // 处理数字(浮点数)字段类型
        else if (propertyType == typeof(NumberFieldType))
        {
            schema["type"] = "number";
            schema["widget"] = "inputNumber";
            schema["x-component"] = "NumberPicker";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(double));

            var props = new JsonObject
            {
                ["precision"] = 2
            };

            // 处理Range特性
            var rangeAttr = property.GetCustomAttribute<RangeAttribute>();
            if (rangeAttr != null)
            {
                // 设置最小值和最大值 - FormRender 2.0 格式
                schema["minimum"] = Convert.ToDouble(rangeAttr.Minimum);
                schema["maximum"] = Convert.ToDouble(rangeAttr.Maximum);
                schema["min"] = Convert.ToDouble(rangeAttr.Minimum);
                schema["max"] = Convert.ToDouble(rangeAttr.Maximum);

                // 组件属性中的min和max
                props["min"] = Convert.ToDouble(rangeAttr.Minimum);
                props["max"] = Convert.ToDouble(rangeAttr.Maximum);
            }
            else
            {
                // 默认范围
                schema["min"] = 1;
                schema["max"] = 100;
            }

            schema["x-component-props"] = props;

            // 添加错误消息
            schema["message"] = new JsonObject
            {
                ["min"] = ""
            };
        }
        // 处理图片字段类型
        else if (propertyType == typeof(ImageFieldType))
        {
            schema["type"] = "string";
            schema["widget"] = "imageUpload";
            schema["x-component"] = "ImageUploader";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(string));

            var props = new JsonObject
            {
                ["listType"] = "picture-card",
                ["accept"] = "image/*"
            };
            schema["x-component-props"] = props;
            schema["props"] = props.DeepClone();
        }
        // 处理文件字段类型
        else if (propertyType == typeof(FileFieldType))
        {
            schema["type"] = "string";
            schema["widget"] = "upload";
            schema["x-component"] = "Upload";

            // 处理默认值
            ProcessDefaultValue(property, schema, typeof(string));

            var props = new JsonObject
            {
                ["listType"] = "text",
                ["multiple"] = false
            };
            schema["x-component-props"] = props;
            schema["props"] = props.DeepClone();
        }
        // 处理数组字段类型
        else if (propertyType == typeof(ArrayFieldType) ||
                 (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ArrayFieldType<>)))
        {
            schema["type"] = "array";
            schema["widget"] = "list";
            schema["x-component"] = "ArrayItems";

            // 根据泛型参数确定数组项的类型
            var itemType = typeof(string); // 默认为字符串类型
            if (propertyType.IsGenericType)
            {
                var genericArgs = propertyType.GetGenericArguments();
                if (genericArgs.Length > 0)
                {
                    itemType = genericArgs[0];
                }
            }

            // 处理默认值
            if (propertyType.IsGenericType)
            {
                // 使用数组类型作为预期类型
                var arrayType = Array.CreateInstance(itemType, 0).GetType();
                ProcessDefaultValue(property, schema, arrayType);
            }
            else
            {
                ProcessDefaultValue(property, schema, typeof(string[]));
            }

            // 根据数组项的类型设置组件
            var items = new JsonObject
            {
                ["type"] = GetJsonSchemaType(itemType)
            };

            // 根据数组项类型选择合适的组件
            if (itemType == typeof(int) || itemType == typeof(long) ||
                itemType == typeof(double) || itemType == typeof(float) ||
                itemType == typeof(decimal))
            {
                items["widget"] = "inputNumber";
                items["x-component"] = "NumberPicker";

                var props = new JsonObject();

                // 处理Range特性
                var rangeAttr = property.GetCustomAttribute<RangeAttribute>();
                if (rangeAttr != null)
                {
                    // 设置最小值和最大值 - FormRender 2.0 格式
                    items["minimum"] = Convert.ToDouble(rangeAttr.Minimum);
                    items["maximum"] = Convert.ToDouble(rangeAttr.Maximum);
                    items["min"] = Convert.ToDouble(rangeAttr.Minimum);
                    items["max"] = Convert.ToDouble(rangeAttr.Maximum);

                    // 组件属性中的min和max
                    props["min"] = Convert.ToDouble(rangeAttr.Minimum);
                    props["max"] = Convert.ToDouble(rangeAttr.Maximum);
                }

                // 对于整数类型设置precision为0
                if (itemType == typeof(int) || itemType == typeof(long))
                {
                    props["precision"] = 0;
                }
                else
                {
                    props["precision"] = 2;
                }

                if (props.Count > 0)
                {
                    items["x-component-props"] = props;
                    items["props"] = props.DeepClone();
                }
            }
            else if (itemType == typeof(bool))
            {
                items["widget"] = "switch";
                items["x-component"] = "Switch";
            }
            else if (itemType == typeof(DateTime))
            {
                items["widget"] = "datePicker";
                items["x-component"] = "DatePicker";
                items["format"] = "date-time";
                var itemProps = new JsonObject
                {
                    ["showTime"] = true
                };
                items["x-component-props"] = itemProps;
                items["props"] = itemProps.DeepClone();
            }
            else
            {
                items["widget"] = "input";
                items["x-component"] = "Input";

                var props = new JsonObject();

                // 处理字符串类型的额外验证
                var stringLengthAttr = property.GetCustomAttribute<StringLengthAttribute>();
                if (stringLengthAttr != null)
                {
                    if (stringLengthAttr.MinimumLength > 0)
                    {
                        items["minLength"] = stringLengthAttr.MinimumLength;
                        items["min"] = stringLengthAttr.MinimumLength;
                    }
                    items["maxLength"] = stringLengthAttr.MaximumLength;
                    items["max"] = stringLengthAttr.MaximumLength;
                    props["maxLength"] = stringLengthAttr.MaximumLength;
                }

                if (props.Count > 0)
                {
                    items["x-component-props"] = props;
                    items["props"] = props.DeepClone();
                }
            }

            schema["items"] = items;
        }

        // 在 GeneratePropertySchema 方法中添加处理 OptionsFieldType 和 OptionsMultiFieldType 的代码
        // 处理枚举单选字段类型
        else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(OptionsFieldType<>))
        {
            schema["type"] = "string";
            schema["widget"] = "radio";
            schema["x-component"] = "Radio.Group";

            // 获取枚举类型
            var enumType = propertyType.GetGenericArguments()[0];

            // 处理默认值
            var defaultValueAttr = property.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValueAttr != null && defaultValueAttr.Value != null)
            {
                try
                {
                    if (defaultValueAttr.Value.GetType().IsEnum)
                    {
                        var enumValue = Convert.ToInt32(defaultValueAttr.Value);
                        schema["default"] = enumValue.ToString();
                        schema["defaultValue"] = enumValue.ToString();
                    }
                }
                catch
                {
                    // 如果转换失败，忽略默认值
                }
            }

            // 创建选项数组
            var options = new JsonArray();
            foreach (var enumValue in Enum.GetValues(enumType))
            {
                var name = enumValue.ToString();
                var description = name;

                // 获取描述特性
                var fieldInfo = enumType.GetField(name!);
                if (fieldInfo != null)
                {
                    var descAttr = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
                    if (descAttr != null && !string.IsNullOrEmpty(descAttr.Description))
                    {
                        description = descAttr.Description;
                    }
                }

                options.Add(new JsonObject
                {
                    ["label"] = description,
                    ["value"] = Convert.ToInt32(enumValue).ToString()
                });
            }

            schema["props"] = new JsonObject
            {
                ["options"] = options
            };

            schema["x-component-props"] = new JsonObject
            {
                ["options"] = options.DeepClone()
            };
        }
        // 处理枚举多选字段类型
        else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(OptionsMultiFieldType<>))
        {
            schema["type"] = "array";
            schema["widget"] = "checkboxes";
            schema["x-component"] = "Checkbox.Group";

            // 获取枚举类型
            var enumType = propertyType.GetGenericArguments()[0];

            // 处理默认值
            var defaultValueAttr = property.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValueAttr != null && defaultValueAttr.Value != null)
            {
                try
                {
                    if (defaultValueAttr.Value.GetType().IsEnum)
                    {
                        var enumValue = Convert.ToInt32(defaultValueAttr.Value);
                        schema["default"] = enumValue.ToString();
                        schema["defaultValue"] = enumValue.ToString();
                    }
                    else if (defaultValueAttr.Value.GetType().IsArray)
                    {
                        var enumValues = ((Array)defaultValueAttr.Value).Cast<object>()
                            .Select(v => Convert.ToInt32(v).ToString())
                            .ToArray();
                        schema["default"] = string.Join(",", enumValues);
                        schema["defaultValue"] = string.Join(",", enumValues);
                    }
                }
                catch
                {
                    // 如果转换失败，忽略默认值
                }
            }

            // 创建选项数组
            var options = new JsonArray();
            foreach (var enumValue in Enum.GetValues(enumType))
            {
                var name = enumValue.ToString();
                var description = name;

                // 获取描述特性
                var fieldInfo = enumType.GetField(name!);
                if (fieldInfo != null)
                {
                    var descAttr = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
                    if (descAttr != null && !string.IsNullOrEmpty(descAttr.Description))
                    {
                        description = descAttr.Description;
                    }
                }

                options.Add(new JsonObject
                {
                    ["label"] = description,
                    ["value"] = Convert.ToInt32(enumValue).ToString()
                });
            }

            schema["props"] = new JsonObject
            {
                ["options"] = options,
                ["direction"] = "row"
            };

            schema["x-component-props"] = new JsonObject
            {
                ["options"] = options.DeepClone(),
                ["direction"] = "row"
            };
        }

        else
        {
            return null;
        }

        // 处理必填属性 - 在FormRender 2.0中，同时使用required数组和字段属性
        var requiredAttr = property.GetCustomAttribute<RequiredAttribute>();
        if (requiredAttr != null)
        {
            schema["required"] = true;

            // 如果有错误消息，则添加到schema中
            if (!string.IsNullOrEmpty(requiredAttr.ErrorMessage))
            {
                if (schema["message"] == null)
                {
                    schema["message"] = new JsonObject();
                }
                ((JsonObject)schema["message"]!)["required"] = requiredAttr.ErrorMessage;
            }
        }

        // 处理比较验证（例如确认密码）
        var compareAttr = property.GetCustomAttribute<CompareAttribute>();
        if (compareAttr != null)
        {
            // FormRender 2.0 自定义验证
            schema["x-validator"] = new JsonObject
            {
                ["validator"] = $"(value) => {{ return value === form.values.{compareAttr.OtherProperty} ? '' : '{compareAttr.ErrorMessage ?? $"必须与{compareAttr.OtherProperty}相同"}'; }}",
                ["depends"] = new JsonArray { compareAttr.OtherProperty }
            };
        }

        return schema;
    }

    // 辅助方法：根据.NET类型获取JSON Schema的类型
    private string GetJsonSchemaType(Type type)
    {
        if (type == typeof(string))
            return "string";
        else if (type == typeof(int) || type == typeof(long) || type == typeof(double) || type == typeof(float) || type == typeof(decimal))
            return "number";
        else if (type == typeof(bool))
            return "boolean";
        else if (type == typeof(DateTime))
            return "string"; // 日期在JSON Schema中通常表示为带有format的字符串
        else if (type.IsArray || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            return "array";
        else
            return "object";
    }

    // 通用方法：处理DefaultValue特性
    private void ProcessDefaultValue(PropertyInfo property, JsonObject schema, Type expectedType)
    {
        var defaultValueAttr = property.GetCustomAttribute<DefaultValueAttribute>();
        if (defaultValueAttr != null && defaultValueAttr.Value != null)
        {
            try
            {
                // 根据不同类型处理默认值
                if (expectedType == typeof(string))
                {
                    schema["default"] = defaultValueAttr.Value.ToString();
                    schema["defaultValue"] = defaultValueAttr.Value.ToString();
                }
                else if (expectedType == typeof(bool) && defaultValueAttr.Value is bool boolValue)
                {
                    schema["default"] = boolValue;
                    schema["defaultValue"] = boolValue;
                }
                else if ((expectedType == typeof(int) || expectedType == typeof(double) ||
                         expectedType == typeof(float) || expectedType == typeof(decimal) ||
                         expectedType == typeof(long)) &&
                         defaultValueAttr.Value is IConvertible)
                {
                    var convertedValue = Convert.ToDouble(defaultValueAttr.Value);
                    schema["default"] = convertedValue;
                    schema["defaultValue"] = convertedValue;
                }
                else if (expectedType == typeof(DateTime) && defaultValueAttr.Value is DateTime dateValue)
                {
                    var dateStr = dateValue.ToString("yyyy-MM-dd");
                    schema["default"] = dateStr;
                    schema["defaultValue"] = dateStr;
                }
                else if (defaultValueAttr.Value.GetType().IsArray && expectedType.IsArray)
                {
                    // 处理数组类型的默认值
                    JsonNode arrayNode = new JsonArray();
                    foreach (var item in (Array)defaultValueAttr.Value)
                    {
                        ((JsonArray)arrayNode).Add(JsonValue.Create(item));
                    }
                    schema["default"] = arrayNode;
                    schema["defaultValue"] = arrayNode.DeepClone();
                }
            }
            catch
            {
                // 如果转换失败，忽略默认值
            }
        }
    }

}
