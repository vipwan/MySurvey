// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Biwen.QuickApi.Contents.Domain;
using Biwen.Settings.Domains;
using Biwen.Settings.SettingStores.EFCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MySurvey.Core.Entities;

namespace MySurvey.Core.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
    IdentityDbContext(options), 
    IBiwenSettingsDbContext,
    IContentDbContext
{
    public DbSet<Setting> Settings { get; set; } = null!;

    public DbSet<Content> Contents { get; set; } = null!;


    // 问卷调查相关
    public DbSet<Survey> Surveys { get; set; }
    public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
    public DbSet<SurveyOption> SurveyOptions { get; set; }
    public DbSet<SurveyAnswer> SurveyAnswers { get; set; }
    public DbSet<SurveyQuestionAnswer> SurveyQuestionAnswers { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // 这一行非常重要，不要删除

        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        //重命名表名
        modelBuilder.Entity<Setting>().ToTable("SystemSettings");
        modelBuilder.Entity<Content>().ToTable("SystemContents");

        // 配置 Survey
        modelBuilder.Entity<Survey>(entity =>
        {
            entity.Property(s => s.Title).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Description).HasMaxLength(500);
            entity.Property(s => s.Status).IsRequired();
            entity.Property(s => s.StartTime).IsRequired();
            entity.Property(s => s.EndTime).IsRequired();
            entity.Property(s => s.UserId).IsRequired();

            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置 SurveyQuestion
        modelBuilder.Entity<SurveyQuestion>(entity =>
        {
            entity.Property(q => q.Title).IsRequired().HasMaxLength(500);
            entity.Property(q => q.Description).HasMaxLength(1000);
            entity.Property(q => q.Type).IsRequired();
            entity.Property(q => q.IsRequired).IsRequired();
            entity.Property(q => q.Order).IsRequired();
            entity.Property(q => q.SurveyId).IsRequired();

            entity.HasOne(q => q.Survey)
                .WithMany(s => s.Questions)
                .HasForeignKey(q => q.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置 SurveyOption
        modelBuilder.Entity<SurveyOption>(entity =>
        {
            entity.Property(o => o.Content).IsRequired().HasMaxLength(500);
            entity.Property(o => o.Value).HasMaxLength(100);
            entity.Property(o => o.Order).IsRequired();
            entity.Property(o => o.QuestionId).IsRequired();

            entity.HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置 SurveyAnswer
        modelBuilder.Entity<SurveyAnswer>(entity =>
        {
            entity.Property(a => a.SurveyId).IsRequired();
            //entity.Property(a => a.UserId).IsRequired();
            entity.Property(a => a.SubmitTime).IsRequired();
            entity.Property(a => a.Status).IsRequired();

            entity.HasIndex(a => new { a.SurveyId, a.UserId }).IsUnique();

            entity.HasOne(a => a.Survey)
                .WithMany(s => s.Answers)
                .HasForeignKey(a => a.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置 SurveyQuestionAnswer
        modelBuilder.Entity<SurveyQuestionAnswer>(entity =>
        {
            entity.Property(a => a.AnswerId).IsRequired();
            entity.Property(a => a.QuestionId).IsRequired();
            entity.Property(a => a.Content).HasMaxLength(2000);

            entity.HasOne(a => a.Answer)
                .WithMany(sa => sa.QuestionAnswers)
                .HasForeignKey(a => a.AnswerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置 MatrixAnswer
        modelBuilder.Entity<MatrixAnswer>(entity =>
        {
            entity.ToTable("SurveyMatrixAnswers");

            entity.Property(a => a.RowId).IsRequired();
            entity.Property(a => a.ColumnId).IsRequired();
            entity.Property(a => a.Value).IsRequired().HasMaxLength(500);

            entity.HasOne(a => a.Row)
                .WithMany()
                .HasForeignKey(a => a.RowId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Column)
                .WithMany()
                .HasForeignKey(a => a.ColumnId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}