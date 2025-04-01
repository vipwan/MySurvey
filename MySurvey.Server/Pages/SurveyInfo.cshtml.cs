// Licensed to the MySurvey.Server under one or more agreements.
// The MySurvey.Server licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySurvey.Core.Entities;
using MySurvey.Core.Interfaces;

namespace MySurvey.Server.Pages
{
    public class SurveyInfoModel : PageModel
    {
        private readonly ISurveyService _surveyService;
        private readonly ILogger<SurveyInfoModel> _logger;

        public SurveyInfoModel(ISurveyService surveyService, ILogger<SurveyInfoModel> logger)
        {
            _surveyService = surveyService;
            _logger = logger;
        }

        // 答案模型
        public class AnswerModel
        {
            public Guid QuestionId { get; set; }
            public string? Value { get; set; }
            public List<string>? Values { get; set; } = [];
        }

        [BindProperty]
        public List<AnswerModel> Answers { get; set; } = [];

        // 添加字典存储用户选择的单选项值，问题ID -> 选项ID
        public Dictionary<Guid, string> SelectedSingleChoiceValues { get; private set; } = new();

        // 添加字典存储用户选择的多选项值，问题ID -> 选项ID列表
        public Dictionary<Guid, List<string>> SelectedMultipleChoiceValues { get; private set; } = new();

        // 添加字典存储用户输入的文本值，问题ID -> 文本内容
        public Dictionary<Guid, string> SelectedTextValues { get; private set; } = new();

        // 添加字典存储用户选择的评分值，问题ID -> 评分值
        public Dictionary<Guid, string> SelectedRatingValues { get; private set; } = new();

        public Survey? Survey { get; private set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync(Guid id)
        {
            Survey = await _surveyService.GetSurveyAsync(id);

            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"]!.ToString();
            }

            // 从TempData恢复用户选择（如果有）
            if (TempData["SelectedSingleChoiceValues"] is string singleChoiceJson && !string.IsNullOrEmpty(singleChoiceJson))
            {
                SelectedSingleChoiceValues = System.Text.Json.JsonSerializer.Deserialize<Dictionary<Guid, string>>(singleChoiceJson)!;
            }

            if (TempData["SelectedMultipleChoiceValues"] is string multipleChoiceJson && !string.IsNullOrEmpty(multipleChoiceJson))
            {
                SelectedMultipleChoiceValues = System.Text.Json.JsonSerializer.Deserialize<Dictionary<Guid, List<string>>>(multipleChoiceJson)!;
            }

            if (TempData["SelectedTextValues"] is string textValuesJson && !string.IsNullOrEmpty(textValuesJson))
            {
                SelectedTextValues = System.Text.Json.JsonSerializer.Deserialize<Dictionary<Guid, string>>(textValuesJson)!;
            }

            if (TempData["SelectedRatingValues"] is string ratingValuesJson && !string.IsNullOrEmpty(ratingValuesJson))
            {
                SelectedRatingValues = System.Text.Json.JsonSerializer.Deserialize<Dictionary<Guid, string>>(ratingValuesJson)!;
            }
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            try
            {
                // 打印接收到的表单数据以便调试
                _logger.LogInformation("接收到的表单数据项数: {Count}", Answers?.Count ?? 0);
                // 保存用户选择，用于验证失败时重新填充表单
                SaveUserSelections();
                // 如果没有接收到任何答案数据，则通过Request.Form直接获取
                if (Answers == null || Answers.Count == 0)
                {
                    _logger.LogWarning("模型绑定失败，尝试直接获取表单数据");
                    // 通过直接读取表单数据来解析答案
                    Answers = [];
                    var formKeys = Request.Form.Keys.Where(k => k.StartsWith("Answers[") && k.EndsWith("].QuestionId"));
                    foreach (var key in formKeys)
                    {
                        // 解析索引值 从 "Answers[0].QuestionId" 提取 "0"
                        var indexMatch = System.Text.RegularExpressions.Regex.Match(key, @"Answers\[(\d+)\]");
                        if (!indexMatch.Success) continue;

                        var index = indexMatch.Groups[1].Value;
                        var questionIdKey = $"Answers[{index}].QuestionId";
                        var valueKey = $"Answers[{index}].Value";
                        var valuesKey = $"Answers[{index}].Values";

                        if (!Guid.TryParse(Request.Form[questionIdKey], out var questionId))
                            continue;

                        var answer1 = new AnswerModel { QuestionId = questionId };

                        // 获取Value
                        if (Request.Form.ContainsKey(valueKey))
                        {
                            answer1.Value = Request.Form[valueKey];

                            // 保存单选或文本输入的值
                            SelectedSingleChoiceValues[questionId] = answer1.Value!;
                            SelectedTextValues[questionId] = answer1.Value!;
                            SelectedRatingValues[questionId] = answer1.Value!;
                        }

                        // 获取Values
                        if (Request.Form.ContainsKey(valuesKey))
                        {
                            answer1.Values = [.. Request.Form[valuesKey]];

                            // 保存多选的值
                            SelectedMultipleChoiceValues[questionId] = answer1.Values;
                        }

                        Answers.Add(answer1);
                    }

                    _logger.LogInformation("直接从表单获取到的答案数: {Count}", Answers.Count);
                }

                // 获取问卷信息
                Survey = await _surveyService.GetSurveyAsync(id);
                if (Survey == null)
                {
                    _logger.LogWarning("问卷不存在，ID: {Id}", id);
                    return NotFound("问卷不存在");
                }

                if (Survey.Status != SurveyStatus.Published)
                {
                    ModelState.AddModelError(string.Empty, "问卷当前不可提交");
                    SaveSelectionsToTempData(); // 保存用户选择到TempData
                    return Page();
                }

                // 创建答卷对象
                var answer = new SurveyAnswer
                {
                    Id = Guid.NewGuid(),
                    SurveyId = id,
                    Survey = Survey,
                    AnonymousId = Guid.NewGuid().ToString(),
                    SubmitTime = DateTime.Now,
                    Status = AnswerStatus.Submitted,
                    QuestionAnswers = []
                };

                // 处理每个问题的答案
                foreach (var answerModel in Answers)
                {
                    var question = Survey.Questions.FirstOrDefault(q => q.Id == answerModel.QuestionId);
                    if (question == null)
                    {
                        _logger.LogWarning("问题不存在，ID: {Id}", answerModel.QuestionId);
                        continue;
                    }

                    var questionAnswer = new SurveyQuestionAnswer
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = question.Id,
                        Question = question,
                        AnswerId = answer.Id,
                        Answer = answer
                    };

                    switch (question.Type)
                    {
                        case QuestionType.SingleChoice:
                            if (!string.IsNullOrEmpty(answerModel.Value) && Guid.TryParse(answerModel.Value, out var optionId))
                            {
                                questionAnswer.OptionIds = new List<Guid> { optionId };
                                var option = question.Options.FirstOrDefault(o => o.Id == optionId);
                                if (option != null)
                                {
                                    questionAnswer.OptionValues = new List<string> { option.Content };
                                }
                            }
                            break;

                        case QuestionType.MultipleChoice:
                            if (answerModel.Values != null && answerModel.Values.Any())
                            {
                                var optionIds = new List<Guid>();
                                var optionContents = new List<string>();

                                foreach (var value in answerModel.Values)
                                {
                                    if (Guid.TryParse(value, out var oid))
                                    {
                                        optionIds.Add(oid);
                                        var option = question.Options.FirstOrDefault(o => o.Id == oid);
                                        if (option != null)
                                        {
                                            optionContents.Add(option.Content);
                                        }
                                    }
                                }

                                questionAnswer.OptionIds = optionIds;
                                questionAnswer.OptionValues = optionContents;
                            }
                            break;

                        case QuestionType.TextInput:
                            questionAnswer.Content = answerModel.Value;
                            break;

                        case QuestionType.Rating:
                            if (!string.IsNullOrEmpty(answerModel.Value) && int.TryParse(answerModel.Value, out var rating))
                            {
                                questionAnswer.RatingValue = rating;
                            }
                            break;
                    }

                    answer.QuestionAnswers.Add(questionAnswer);
                }

                // 验证必填项
                var requiredQuestions = Survey.Questions.Where(q => q.IsRequired).ToList();
                foreach (var question in requiredQuestions)
                {
                    var questionAnswer = answer.QuestionAnswers.FirstOrDefault(qa => qa.QuestionId == question.Id);
                    if (questionAnswer == null)
                    {
                        ModelState.AddModelError(string.Empty, $"问题 '{question.Title}' 为必填项，请回答。");
                        SaveSelectionsToTempData(); // 保存用户选择到TempData
                        return Page();
                    }

                    // 验证答案是否有效
                    bool isValid = false;
                    switch (question.Type)
                    {
                        case QuestionType.SingleChoice:
                            isValid = questionAnswer.OptionIds != null && questionAnswer.OptionIds.Any();
                            break;
                        case QuestionType.MultipleChoice:
                            isValid = questionAnswer.OptionIds != null && questionAnswer.OptionIds.Any();
                            break;
                        case QuestionType.TextInput:
                            isValid = !string.IsNullOrWhiteSpace(questionAnswer.Content);
                            break;
                        case QuestionType.Rating:
                            isValid = questionAnswer.RatingValue.HasValue;
                            break;
                    }

                    if (!isValid)
                    {
                        ModelState.AddModelError(string.Empty, $"问题 '{question.Title}' 为必填项，请提供有效答案。");
                        SaveSelectionsToTempData(); // 保存用户选择到TempData
                        return Page();
                    }
                }

                // 提交答卷
                _logger.LogInformation("准备提交答卷，问卷ID: {SurveyId}，答案数量: {AnswerCount}",
                    id, answer.QuestionAnswers.Count);

                await _surveyService.SubmitSurveyAnswerAsync(answer);
                _logger.LogInformation("答卷提交成功，问卷ID: {SurveyId}", id);

                // 提交成功，清除保存的用户选择
                ClearUserSelections();

                TempData["SuccessMessage"] = "问卷提交成功，感谢您的参与！";
                return RedirectToPage("/SurveyInfo", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "提交问卷时发生错误");
                ErrorMessage = "提交失败：" + ex.Message;
                ModelState.AddModelError(string.Empty, ErrorMessage);
                SaveSelectionsToTempData(); // 保存用户选择到TempData
                return Page();
            }
        }

        // 从表单数据中保存用户选择到字典
        private void SaveUserSelections()
        {
            // 清空之前的选择
            SelectedSingleChoiceValues.Clear();
            SelectedMultipleChoiceValues.Clear();
            SelectedTextValues.Clear();
            SelectedRatingValues.Clear();

            if (Answers == null) return;

            foreach (var answer in Answers)
            {
                var questionId = answer.QuestionId;

                // 保存单选值和评分值
                if (!string.IsNullOrEmpty(answer.Value))
                {
                    SelectedSingleChoiceValues[questionId] = answer.Value;
                    SelectedRatingValues[questionId] = answer.Value;
                    SelectedTextValues[questionId] = answer.Value;
                }

                // 保存多选值
                if (answer.Values != null && answer.Values.Any())
                {
                    SelectedMultipleChoiceValues[questionId] = answer.Values;
                }
            }
        }

        // 将用户选择保存到TempData
        private void SaveSelectionsToTempData()
        {
            if (SelectedSingleChoiceValues.Any())
            {
                TempData["SelectedSingleChoiceValues"] = System.Text.Json.JsonSerializer.Serialize(SelectedSingleChoiceValues);
            }

            if (SelectedMultipleChoiceValues.Any())
            {
                TempData["SelectedMultipleChoiceValues"] = System.Text.Json.JsonSerializer.Serialize(SelectedMultipleChoiceValues);
            }

            if (SelectedTextValues.Any())
            {
                TempData["SelectedTextValues"] = System.Text.Json.JsonSerializer.Serialize(SelectedTextValues);
            }

            if (SelectedRatingValues.Any())
            {
                TempData["SelectedRatingValues"] = System.Text.Json.JsonSerializer.Serialize(SelectedRatingValues);
            }
        }

        // 清除保存的用户选择
        private void ClearUserSelections()
        {
            SelectedSingleChoiceValues.Clear();
            SelectedMultipleChoiceValues.Clear();
            SelectedTextValues.Clear();
            SelectedRatingValues.Clear();

            TempData.Remove("SelectedSingleChoiceValues");
            TempData.Remove("SelectedMultipleChoiceValues");
            TempData.Remove("SelectedTextValues");
            TempData.Remove("SelectedRatingValues");
        }
    }
}
