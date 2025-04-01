using System.IO;

namespace MySurvey.Core.Interfaces;

/// <summary>
/// 导出服务接口
/// </summary>
public interface IExportService
{
    /// <summary>
    /// 导出问卷答卷为Excel
    /// </summary>
    /// <param name="surveyId">问卷ID</param>
    /// <param name="userId">UserId</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Excel文件流</returns>
    Task<MemoryStream> ExportSurveyAnswersToExcelAsync(Guid surveyId,string userId, CancellationToken cancellationToken = default);
} 