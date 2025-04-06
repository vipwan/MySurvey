// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.FieldTypes;

namespace MySurvey.Core.Contents;

/// <summary>
/// 定义一个普通页面类型
/// </summary>
public class InfoPage : ContentBase<InfoPage>
{

    [Display(Name = "标题")]
    public TextFieldType Title { get; set; } = null!;

    [Display(Name = "内容")]
    [MarkdownToolBar(MarkdownToolStyle.Full)]
    public MarkdownFieldType Content { get; set; } = null!;

    [Display(Name = "描述")]
    public TextAreaFieldType? Description { get; set; } = null!;

    #region IContent

    public override string Content_Description => "这是一个普通的文档类型";


    #endregion
}