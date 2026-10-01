using System.Text.Json;
using NovinApp.Shared.DTOs.Common;

namespace NovinApp.Client.Helpers
{
    public static class ApiResponseHelper
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<ApiResponse<T>> ParseApiResponseAsync<T>(this HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return response.IsSuccessStatusCode
                    ? ApiResponse<T>.Ok(default!, "عملیات با موفقیت انجام شد.")
                    : ApiResponse<T>.Fail($"خطای سرور (کد {(int)response.StatusCode})");
            }

            try
            {
                var apiRes = JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
                if (apiRes != null && (apiRes.Success || !string.IsNullOrEmpty(apiRes.Message) || apiRes.Data != null))
                {
                    return apiRes;
                }
            }
            catch
            {
                // Try problem details or plain string
            }

            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                {
                    var errorMessages = new List<string>();
                    foreach (var prop in errorsProp.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in prop.Value.EnumerateArray())
                            {
                                var msg = item.GetString();
                                if (!string.IsNullOrEmpty(msg)) errorMessages.Add(msg);
                            }
                        }
                        else
                        {
                            var msg = prop.Value.GetString();
                            if (!string.IsNullOrEmpty(msg)) errorMessages.Add(msg);
                        }
                    }

                    if (errorMessages.Count > 0)
                    {
                        return ApiResponse<T>.Fail(string.Join(" | ", errorMessages));
                    }
                }

                if (root.TryGetProperty("title", out var titleProp))
                {
                    var title = titleProp.GetString();
                    if (!string.IsNullOrEmpty(title))
                        return ApiResponse<T>.Fail(title);
                }
            }
            catch
            {
                // Raw string response
            }

            if (response.IsSuccessStatusCode)
            {
                return ApiResponse<T>.Ok(default!, content);
            }

            return ApiResponse<T>.Fail(content);
        }
    }
}
