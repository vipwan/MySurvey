// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi.Contents;
using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MySurvey.Core.Entities;

namespace MySurvey.Core.Data;

public static class IdentitySeed
{
    const string DefaultUser = "vipwan@sina.com";
    const string DefaultPassword = "123456";

    /// <summary>
    /// 初始化默认用户
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    /// <returns>异步任务</returns>
    public static async Task SeedDefaultUserAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        // 检查是否存在用户
        if (userManager.Users.Any())
        {
            logger.LogInformation("已存在用户，跳过初始化默认用户");
            return;
        }

        logger.LogInformation("开始初始化默认用户");

        // 创建默认用户
        var defaultUser = new IdentityUser
        {
            UserName = DefaultUser,
            Email = DefaultUser,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(defaultUser, DefaultPassword);

        if (result.Succeeded)
        {
            logger.LogInformation("默认用户创建成功");
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("创建默认用户失败: {Errors}", errors);
            throw new Exception($"创建默认用户失败: {errors}");
        }
    }

    /// <summary>
    /// 初始化默认问卷, 用于开发环境.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static async Task SeedDefaultSurveyAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // 检查是否已存在问卷
        if (await dbContext.Surveys.AnyAsync())
        {
            logger.LogInformation("已存在问卷，跳过初始化默认问卷");
            return;
        }

        logger.LogInformation("开始初始化默认问卷");

        // 获取默认用户
        var defaultUser = await userManager.FindByEmailAsync(DefaultUser);
        if (defaultUser == null)
        {
            logger.LogError("找不到默认用户，请先初始化默认用户");
            return;
        }

        // 创建11个问卷，分别对应三种状态
        var surveyTitles = new[]
        {
        "客户满意度调查", "产品体验反馈", "员工敬业度调查", "市场调研问卷",
        "培训需求调查", "活动反馈调查", "网站用户体验调查", "教育教学评估",
        "健康生活习惯调查", "旅游体验反馈", "服务质量评价"
    };

        var surveys = new List<Survey>();
        for (int i = 0; i < 11; i++)
        {
            // 轮流使用三种状态：0-草稿，1-已发布，2-已结束
            var status = (SurveyStatus)(i % 3);

            var startTime = DateTime.Now.AddDays(-5);
            var endTime = status == SurveyStatus.Ended
                ? DateTime.Now.AddDays(-1)
                : DateTime.Now.AddDays(15);

            var survey = new Survey
            {
                Id = Guid.NewGuid(),
                Title = surveyTitles[i],
                Description = $"这是一个{surveyTitles[i]}问卷，用于收集相关反馈和意见。",
                Status = status,
                StartTime = startTime,
                EndTime = endTime,
                UserId = defaultUser.Id,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // 创建5个问题，包括单选、多选、填空和评分题
            CreateDefaultQuestions(survey);

            surveys.Add(survey);
        }

        // 保存所有问卷到数据库
        await dbContext.Surveys.AddRangeAsync(surveys);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("默认问卷初始化完成，共创建了 {Count} 个问卷", surveys.Count);
    }

    /// <summary>
    /// 为问卷创建默认问题
    /// </summary>
    private static void CreateDefaultQuestions(Survey survey)
    {
        // 为每个问卷创建5个问题，确保包含单选、多选、填空和评分题
        var questions = new List<SurveyQuestion>
    {
        // 问题1：单选题
        new ()
        {
            Id = Guid.NewGuid(),
            SurveyId = survey.Id,
            Title = $"您对{survey.Title.TrimEnd("问卷".ToCharArray())}的整体满意度如何？",
            Description = "请选择最符合您感受的选项",
            Type = QuestionType.SingleChoice,
            IsRequired = true,
            Order = 1,
            Options =
            [
                new() { Id = Guid.NewGuid(), Content = "非常满意", Order = 1 },
                new() { Id = Guid.NewGuid(), Content = "满意", Order = 2 },
                new () { Id = Guid.NewGuid(), Content = "一般", Order = 3 , IsDefault =true },
                new () { Id = Guid.NewGuid(), Content = "不满意", Order = 4 },
                new () { Id = Guid.NewGuid(), Content = "非常不满意", Order = 5 }
            ]
        },
        
        // 问题2：多选题
        new ()
        {
            Id = Guid.NewGuid(),
            SurveyId = survey.Id,
            Title = $"您最看重{survey.Title.TrimEnd("问卷".ToCharArray())}的哪些方面？（可多选）",
            Description = "请选择所有适用的选项",
            Type = QuestionType.MultipleChoice,
            IsRequired = true,
            Order = 2,
            Options =
            [
                new SurveyOption { Id = Guid.NewGuid(), Content = "质量", Order = 1 },
                new SurveyOption { Id = Guid.NewGuid(), Content = "价格", Order = 2 , IsDefault = true },
                new SurveyOption { Id = Guid.NewGuid(), Content = "服务", Order = 3 , IsDefault = true },
                new SurveyOption { Id = Guid.NewGuid(), Content = "体验", Order = 4 },
                new SurveyOption { Id = Guid.NewGuid(), Content = "创新", Order = 5 },
                new SurveyOption { Id = Guid.NewGuid(), Content = "其他", Order = 6 }
            ]
        },
        
        // 问题3：填空题
        new SurveyQuestion
        {
            Id = Guid.NewGuid(),
            SurveyId = survey.Id,
            Title = "您对我们有什么建议或意见？",
            Description = "请详细描述您的想法",
            Type = QuestionType.TextInput,
            IsRequired = false,
            Order = 3
        },
        
        // 问题4：评分题
        new SurveyQuestion
        {
            Id = Guid.NewGuid(),
            SurveyId = survey.Id,
            Title = "请对我们的服务质量进行评分",
            Description = "1分最低，5分最高",
            Type = QuestionType.Rating,
            IsRequired = true,
            Order = 4
        },
        
        // 问题5：单选题（再来一个不同的单选题）
        new SurveyQuestion
        {
            Id = Guid.NewGuid(),
            SurveyId = survey.Id,
            Title = "您是否愿意向朋友推荐我们？",
            Description = "请选择一个选项",
            Type = QuestionType.SingleChoice,
            IsRequired = true,
            Order = 5,
            Options =
            [
                new SurveyOption { Id = Guid.NewGuid(), Content = "一定会推荐", Order = 1 },
                new SurveyOption { Id = Guid.NewGuid(), Content = "可能会推荐", Order = 2 },
                new SurveyOption { Id = Guid.NewGuid(), Content = "不确定", Order = 3,IsDefault=true },
                new SurveyOption { Id = Guid.NewGuid(), Content = "可能不会推荐", Order = 4 },
                new SurveyOption { Id = Guid.NewGuid(), Content = "一定不会推荐", Order = 5 }
            ]
        }
    };

        survey.Questions = questions;
    }


    /// <summary>
    /// 创建默认内容
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <summary>
    /// 创建默认内容
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <summary>
    /// 创建默认内容
    /// </summary>
    /// <param name="serviceProvider"></param>
    public static async Task SeedDefaultContentsAsync(IServiceProvider serviceProvider)
    {
        //创建默认的文档:
        //关于我们,服务条款,隐私政策,帮助中心,常见问题,联系我们

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var serializer = scope.ServiceProvider.GetRequiredService<ContentSerializer>();

        // 检查是否已存在内容
        var existingContents = await dbContext.Contents
            .Where(c => c.ContentType == typeof(Biwen.QuickApi.Contents.SamplePage).FullName)
            .ToListAsync();

        if (existingContents.Any())
        {
            logger.LogInformation("已存在内容页面，跳过初始化默认内容");
            return;
        }

        logger.LogInformation("开始初始化默认内容页面");

        // 使用固定的 Id 确保幂等性
        var contentTypes = new[]
        {
        new { Id = new Guid("7F95A120-C65E-4FB1-9CA0-07C5A4331095"), Title = "关于我们", Slug = "about-us" },
        new { Id = new Guid("C6D11D28-E8C2-4CD2-ADFC-5322A1C208C5"), Title = "服务条款", Slug = "terms-of-service" },
        new { Id = new Guid("BF2ACBAA-DA93-4AED-92D8-767E2D3B8B72"), Title = "隐私政策", Slug = "privacy-policy" },
        new { Id = new Guid("3D5C3AC9-6354-4B4E-A6ED-508812120C8F"), Title = "帮助中心", Slug = "help-center" },
        new { Id = new Guid("E8F9E87B-DEF1-40F0-BB80-A919FAB95A2B"), Title = "常见问题", Slug = "faq" },
        new { Id = new Guid("F67D329E-F54F-4E3B-BE0C-2E0E3652D9F3"), Title = "联系我们", Slug = "contact-us" }
    };

        var contents = new List<Content>();

        foreach (var item in contentTypes)
        {
            // 创建 SamplePage 实例 - 注意我们使用 HTML 格式的内容
            var samplePage = new SamplePage
            {
                // 标题是纯文本，保持不变
                Title = new Biwen.QuickApi.Contents.FieldTypes.TextFieldType { Value = item.Title },

                // Description 使用 HTML 格式
                Description = new Biwen.QuickApi.Contents.FieldTypes.MarkdownFieldType
                {
                    Value = $"<p>这是{item.Title}的描述内容</p>"
                },

                // Content 使用更丰富的 HTML 格式
                Content = new Biwen.QuickApi.Contents.FieldTypes.MarkdownFieldType
                {
                    Value = $@"<h1>{item.Title}</h1>
<p>欢迎访问我们的{item.Title}页面。</p>
<p>这是{item.Title}的详细内容，包含了关于我们系统的重要信息。</p>
<h2>联系方式</h2>
<p>如果您有任何问题或建议，请随时与我们联系。</p>
<ul>
  <li>电子邮件：vipwan@example.com</li>
  <li>电话：+86 123-4567-8900</li>
  <li>地址：深圳市南山区</li>
</ul>"
                },

                // 标签数组
                Tags = new Biwen.QuickApi.Contents.FieldTypes.ArrayFieldType
                {
                    Value = "官方,文档,帮助"
                }
            };

            // 创建 Content 实体
            var content = new Content
            {
                Id = item.Id, // 使用预定义的 Id
                Title = item.Title,
                Slug = item.Slug,
                Status = ContentStatus.Published,
                CreatedAt = DateTime.Now,
                PublishedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ContentType = typeof(SamplePage).FullName ?? "Biwen.QuickApi.Contents.SamplePage",
                JsonContent = serializer.SerializeContent(samplePage)
            };

            contents.Add(content);
        }

        // 直接使用 ApplicationDbContext 添加到数据库
        await dbContext.Contents.AddRangeAsync(contents);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("默认内容页面初始化完成，共创建 {Count} 个页面", contents.Count);
    }


}