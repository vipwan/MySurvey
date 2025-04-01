// Licensed to the MySurvey.Server under one or more agreements.
// The MySurvey.Server licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Mapster;
using MySurvey.Core.Entities;

namespace MySurvey.Server.ViewModels;

/// <summary>
/// 配置ViewModel和领域模型的映射
/// </summary>
public class SurveyViewModelsConfigure : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // 注册 Survey 到 SurveyViewModel 的映射
        config.NewConfig<Survey, SurveyViewModel>()
            .Map(dest => dest.QuestionCount, src => src.Questions.Count)
            .Map(dest => dest.AnswerCount, src => src.Answers.Count);

        // 注册 Survey 到 PublicSurveyViewModel 的映射
        config.NewConfig<Survey, PublicSurveyViewModel>();

        // 注册 Survey 到 SurveyDetailViewModel 的映射
        config.NewConfig<Survey, SurveyDetailViewModel>()
            .Map(dest => dest.Questions, src => src.Questions.OrderBy(q => q.Order));

        // 注册 SurveyQuestion 到 SurveyQuestionViewModel 的映射
        config.NewConfig<SurveyQuestion, SurveyQuestionViewModel>()
            .Map(dest => dest.Options, src => src.Options.OrderBy(o => o.Order));

        // 注册 SurveyOption 到 SurveyOptionViewModel 的映射
        config.NewConfig<SurveyOption, SurveyOptionViewModel>();

        // 注册 CreateOptionRequest 到 SurveyOption 的映射
        config.NewConfig<CreateOptionRequest, SurveyOption>();

        // 注册 SurveyAnswer 到 SurveyAnswerViewModel 的映射
        config.NewConfig<SurveyAnswer, SurveyAnswerViewModel>()
            .Map(dest => dest.QuestionCount, src => src.QuestionAnswers.Count);

        // 注册 SurveyAnswer 到 SurveyAnswerDetailViewModel 的映射
        config.NewConfig<SurveyAnswer, SurveyAnswerDetailViewModel>();

        // 注册 SurveyQuestionAnswer 到 QuestionAnswerViewModel 的映射
        config.NewConfig<SurveyQuestionAnswer, QuestionAnswerViewModel>()
            .Map(dest => dest.QuestionTitle, src => src.Question.Title)
            .Map(dest => dest.OptionIds, src => src.OptionIds.ToList())
            .Map(dest => dest.OptionValues, src => src.OptionValues.ToList());

        // 注册 MatrixAnswer 到 MatrixAnswerViewModel 的映射
        config.NewConfig<MatrixAnswer, MatrixAnswerViewModel>()
            .Map(dest => dest.RowContent, src => src.Row.Content)
            .Map(dest => dest.ColumnContent, src => src.Column.Content);

        // 注册 SubmitMatrixAnswerRequest 到 MatrixAnswer 的映射
        config.NewConfig<SubmitMatrixAnswerRequest, MatrixAnswer>()
            .Ignore(dest => dest.Row)
            .Ignore(dest => dest.Column);

        // 注册 SubmitQuestionAnswerRequest 到 SurveyQuestionAnswer 的映射
        config.NewConfig<SubmitQuestionAnswerRequest, SurveyQuestionAnswer>()
            .Ignore(dest => dest.Question)
            .Ignore(dest => dest.Answer)
            .Map(dest => dest.OptionIds, src => src.OptionIds ?? new List<Guid>())
            .Map(dest => dest.OptionValues, src => src.OptionValues ?? new List<string>())
            .Map(dest => dest.MatrixAnswers, src => src.MatrixAnswers);
    }
}
