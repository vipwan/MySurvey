// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.FieldTypes;

namespace MySurvey.Core.Contents;

public class Question : ContentBase<Question>
{
    [Display(Name = "问题")]
    [Required, StringLength(200, MinimumLength = 2)]
    public TextFieldType Q { get; set; } = null!;

    [Display(Name = "答案")]
    [Required]
    public MarkdownFieldType? A { get; set; } = null!;

    [Description("背景色")]
    public ColorFieldType? BackColor { get; set; }

}