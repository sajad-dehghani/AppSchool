using System.Collections.Generic;
using System.Threading.Tasks;
using NovinApp.Shared.DTOs.Exams;

namespace NovinApp.Server.Services
{
    public interface IExamReportService
    {
        Task<List<ExamDto>> GetExamsAsync(string? searchTerm = null);
        Task<ExamDto?> GetExamByIdAsync(int id);
        Task<ExamDto> CreateExamAsync(CreateExamDto dto);
        Task<ExamDto?> UpdateExamAsync(int id, UpdateExamDto dto);
        Task<bool> DeleteExamAsync(int id);

        Task<List<ExamStudentResultItemDto>> GetExamResultsAsync(
            int examId, int? schoolId = null, int? consultantId = null, string? search = null);

        Task<ReportCardDto?> GetStudentReportCardAsync(int examId, int studentProfileId);
        Task<ReportCardDto?> GetStudentReportCardByNationalCodeAsync(int examId, string nationalCode);
        Task<List<StudentExamHistoryItemDto>> GetStudentExamHistoryAsync(int studentProfileId);
        Task<ExamAnalyticsDto?> GetExamAnalyticsAsync(int examId);
        Task<bool> CalculateRanksAndAveragesAsync(int examId);
    }
}
