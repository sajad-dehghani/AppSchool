using System;

namespace NovinApp.Shared
{
    public class ExcelBulkUploadResult
    {
        public bool Success { get; set; }
        public int TotalRows { get; set; }
        public double ElapsedSeconds { get; set; }
        public double RowsPerSecond { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    public class UploadBatchSummary
    {
        public DateTime UploadTime { get; set; }
        public string BatchId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int RecordCount { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public int MinId { get; set; }
        public int MaxId { get; set; }
    }

    public class DeleteBatchResult
    {
        public bool Success { get; set; }
        public int DeletedCount { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
