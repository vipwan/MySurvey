// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-06 17:31:59 SamplePage.cs

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.FieldTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biwen.QuickApi.Contents;


[Description("普通页面")]
public class SamplePage : ContentBase<SamplePage>
{
    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    public TextFieldType Title { get; set; } = null!;


    /// <summary>
    /// 描述
    /// </summary>
    [Display(Name = "描述")]
    [MarkdownToolBar(MarkdownToolStyle.Simple)]
    public MarkdownFieldType? Description { get; set; } = null!;

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "内容")]
    [MarkdownToolBar(MarkdownToolStyle.Standard)]
    public MarkdownFieldType Content { get; set; } = null!;


    [Display(Name = "Tags")]
    [Description("Tags")]
    public ArrayFieldType Tags { get; set; } = null!;


    public override string Content_Description => "这是一个普通的文档类型";

    /// <summary>
    /// 默认Page排序为最前
    /// </summary>
    public override int Content_Order => -1;
}
