// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.EntityFrameworkCore;
using MySurvey.Core.Data;
using MySurvey.Core.Entities;
using MySurvey.Core.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace MySurvey.Core.Services;

/// <summary>
/// 导出服务实现
/// </summary>
[AutoInject<IExportService>]
public class ExportService(ApplicationDbContext context) : IExportService
{

    /// <summary>
    /// 导出问卷答卷为Excel
    /// </summary>
    public async Task<MemoryStream> ExportSurveyAnswersToExcelAsync(Guid surveyId,string userId, CancellationToken cancellationToken = default)
    {
        // 获取问卷信息
        var survey = await context.Surveys
            .Where(x=>x.UserId == userId)//必须是当前用户的问卷
            .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == surveyId, cancellationToken);

        if (survey == null)
        {
            throw new KeyNotFoundException($"问卷 {surveyId} 不存在");
        }

        // 获取所有答卷
        var answers = await context.SurveyAnswers
            .Include(a => a.QuestionAnswers)
                .ThenInclude(qa => qa.Question)
                    .ThenInclude(q => q.Options)
            .Include(a => a.QuestionAnswers)
                .ThenInclude(qa => qa.MatrixAnswers)
                    .ThenInclude(ma => ma.Row)
            .Include(a => a.QuestionAnswers)
                .ThenInclude(qa => qa.MatrixAnswers)
                    .ThenInclude(ma => ma.Column)
            .Where(a => a.SurveyId == surveyId)
            .OrderByDescending(a => a.SubmitTime)
            .ToListAsync(cancellationToken);

        // 创建选项映射字典，提高性能
        var optionsMap = survey.Questions
            .SelectMany(q => q.Options)
            .ToDictionary(o => o.Id, o => o);

        // 创建Excel包
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("答卷数据");

        // 设置表头
        worksheet.Cells[1, 1].Value = "提交时间";
        worksheet.Cells[1, 2].Value = "完成时间";
        worksheet.Cells[1, 3].Value = "状态";
        worksheet.Cells[1, 4].Value = "匿名ID";

        // 添加问题标题作为列
        var questionColumns = new Dictionary<Guid, int>();
        var currentColumn = 5;
        foreach (var question in survey.Questions.OrderBy(q => q.Order))
        {
            questionColumns[question.Id] = currentColumn;
            worksheet.Cells[1, currentColumn].Value = question.Title;
            currentColumn++;
        }

        // 设置表头样式
        using (var range = worksheet.Cells[1, 1, 1, currentColumn - 1])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
        }

        // 填充数据
        var currentRow = 2;
        foreach (var answer in answers)
        {
            worksheet.Cells[currentRow, 1].Value = answer.SubmitTime.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[currentRow, 2].Value = answer.CompleteTime?.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[currentRow, 3].Value = answer.Status.ToString();
            worksheet.Cells[currentRow, 4].Value = answer.AnonymousId;

            // 填充答案
            foreach (var questionAnswer in answer.QuestionAnswers)
            {
                var column = questionColumns[questionAnswer.QuestionId];
                var value = FormatAnswerValue(questionAnswer, optionsMap);
                worksheet.Cells[currentRow, column].Value = value;
            }

            currentRow++;
        }

        // 自动调整列宽
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        // 转换为内存流
        var stream = new MemoryStream();
        await package.SaveAsAsync(stream, cancellationToken);
        stream.Position = 0;

        return stream;
    }

    /// <summary>
    /// 格式化答案值
    /// </summary>
    private string FormatAnswerValue(SurveyQuestionAnswer answer, Dictionary<Guid, SurveyOption> optionsMap)
    {
        // 填空题
        if (answer.Content != null)
        {
            return answer.Content;
        }

        // 单选题和多选题
        if (answer.OptionIds != null && answer.OptionIds.Any())
        {
            var options = new List<string>();
            foreach (var optionId in answer.OptionIds)
            {
                if (optionsMap.TryGetValue(optionId, out var option))
                {
                    options.Add(option.Content);
                }
            }
            return string.Join(", ", options);
        }

        // 评分题
        if (answer.RatingValue.HasValue)
        {
            return answer.RatingValue.Value.ToString();
        }

        // 矩阵题
        if (answer.MatrixAnswers != null && answer.MatrixAnswers.Any())
        {
            var matrixValues = answer.MatrixAnswers
                .Select(ma => $"{ma.Row.Content}: {ma.Column.Content}")
                .ToList();
            return string.Join("; ", matrixValues);
        }

        return string.Empty;
    }
}
