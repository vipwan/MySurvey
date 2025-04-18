// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.FieldTypes;

namespace MySurvey.Core.Contents;

public class Question : ContentBase<Question>
{

    [DisplayName("时间HH:mm:ss")]
    public TimeFieldType? Time { get; set; } = null!;


    [DisplayName("图片")]
    [Description("图片")]
    public ImageFieldType? Image { get; set; } = null!;


    [Display(Name = "问题")]
    [Required, StringLength(200, MinimumLength = 2)]
    public TextFieldType Q { get; set; } = null!;

    [Display(Name = "答案")]
    [Required]
    [MarkdownToolBar(MarkdownToolStyle.Standard)]
    public MarkdownFieldType? A { get; set; }

    [DisplayName("答案2")]
    [MarkdownToolBar(MarkdownToolStyle.Simple)]
    public MarkdownFieldType? B { get; set; }


    [Description("背景色")]
    public ColorFieldType? BackColor { get; set; }


    [Description("是否置顶")]
    public BooleanFieldType? IsTop { get; set; }

    [DisplayName("问题分类")]
    [Description("问题分类")]
    public OptionsFieldType<QuestionType>? Category { get; set; }

    [Display(Name = "多选题")]
    [Description("问题分类.复选")]
    public OptionsMultiFieldType<QuestionType>? CategoryMulti { get; set; }

    [Required]
    [DisplayName("创建时间")]
    public DateTimeFieldType? CreatedAt { get; set; } = null!;

    //[DisplayName("数字")]
    //public NumberFieldType? Number { get; set; } = null!;

    [DisplayName("整数")]
    [Range(1, 10)]
    public IntegerFieldType? Integer { get; set; }

    [Required]
    [DisplayName("请求地址")]
    public UrlFieldType? Url { get; set; } = null!;

    [DisplayName("文件")]
    [Description("文件")]
    public FileFieldType? File { get; set; } = null!;


}

[Description("问题分类")]
public enum QuestionType
{
    [Description("单选")]
    SingleChoice = 1,
    [Description("多选")]
    MultipleChoice = 2,
    [Description("文本")]
    Text = 3,
    [Description("图片")]
    Image = 4,
    [Description("视频")]
    Video = 5,
    [Display(Name = "音频1")]
    Audio = 6,
}
