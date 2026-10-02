using System.Text.Json.Serialization;

namespace NovinApp.Shared.DTOs.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Errors { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "") =>
            new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> Fail(string message, object? errors = null) =>
            new() { Success = false, Message = message, Errors = errors };

        public static ApiResponse<T> SuccessResult(T? data = default, string message = "") =>
            new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> FailureResult(string message, object? errors = null) =>
            new() { Success = false, Message = message, Errors = errors };
    }
}
