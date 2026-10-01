// Server/Services/ZarinpalService.cs
using System.Text.Json;
using NovinApp.Shared;

namespace NovinApp.Server.Services;

public class ZarinpalService
{
    private readonly HttpClient _httpClient;
    private readonly ZarinPalOptions _options;
    private readonly ILogger<ZarinpalService> _logger;
    private readonly IConfiguration _configuration;
    private static readonly Dictionary<string, long> _pendingPayments = new();

    public ZarinpalService(
        HttpClient httpClient,
        ZarinPalOptions options,
        ILogger<ZarinpalService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<PaymentResponseDto> RequestPayment(PaymentRequestDto request)
    {
        // 1. اعتبارسنجی MerchantId
        if (string.IsNullOrWhiteSpace(_options.MerchantId))
        {
            _logger.LogError("MerchantId is empty in configuration.");
            return new PaymentResponseDto
            {
                IsSuccess = false,
                Message = "MerchantId در تنظیمات یافت نشد."
            };
        }

        // 2. تعیین آدرس بر اساس محیط
        var isDevelopment = _options.IsDevelopment;
        var apiUrl = isDevelopment
            ? "https://sandbox.zarinpal.com/pg/v4/payment/request.json"
            : "https://api.zarinpal.com/pg/v4/payment/request.json";

        // 3. ساخت Callback URL
        var appUrl = _configuration["AppUrl"];
        if (string.IsNullOrWhiteSpace(appUrl))
        {
            _logger.LogError("AppUrl is not configured.");
            return new PaymentResponseDto
            {
                IsSuccess = false,
                Message = "آدرس بازگشت از درگاه تنظیم نشده است."
            };
        }
        var callbackUrl = $"{appUrl}/api/payment/callback";

        var payload = new
        {
            merchant_id = _options.MerchantId,
            amount = request.Amount,
            description = request.Description,
            callback_url = callbackUrl,
            currency = "IRR"
        };

        try
        {
            _logger.LogInformation("Sending request to Zarinpal: {Url} with payload: {Payload}", apiUrl, JsonSerializer.Serialize(payload));

            var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Zarinpal response status: {StatusCode}, content: {Content}", response.StatusCode, content);

            // 4. بررسی وضعیت HTTP
            if (!response.IsSuccessStatusCode)
            {
                return new PaymentResponseDto
                {
                    IsSuccess = false,
                    Message = $"خطا در ارتباط با درگاه. کد وضعیت: {response.StatusCode}"
                };
            }

            // 5. بررسی صحت JSON
            if (!IsValidJson(content))
            {
                _logger.LogError("Invalid JSON received: {Content}", content);
                return new PaymentResponseDto
                {
                    IsSuccess = false,
                    Message = "پاسخ دریافتی از درگاه معتبر نیست."
                };
            }

            var result = JsonSerializer.Deserialize<ZarinpalRequestResponse>(content);

            if (result?.Data != null && result.Data.Code == 100)
            {
                // ذخیره مبلغ برای مرحله تایید
                lock (_pendingPayments)
                {
                    _pendingPayments[result.Data.Authority] = request.Amount;
                }

                var paymentUrl = isDevelopment
                    ? $"https://sandbox.zarinpal.com/pg/StartPay/{result.Data.Authority}"
                    : $"https://www.zarinpal.com/pg/StartPay/{result.Data.Authority}";

                return new PaymentResponseDto
                {
                    IsSuccess = true,
                    Authority = result.Data.Authority,
                    PaymentUrl = paymentUrl,
                    Message = "درخواست پرداخت با موفقیت ثبت شد."
                };
            }

            var errorMsg = result?.Error?.Message ?? $"کد خطا: {result?.Data?.Code}";
            return new PaymentResponseDto
            {
                IsSuccess = false,
                Message = $"خطا در ثبت درخواست: {errorMsg}"
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error.");
            return new PaymentResponseDto
            {
                IsSuccess = false,
                Message = "خطا در تجزیه پاسخ درگاه پرداخت."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in RequestPayment.");
            return new PaymentResponseDto
            {
                IsSuccess = false,
                Message = $"خطا: {ex.Message}"
            };
        }
    }

    public async Task<PaymentVerificationResultDto> VerifyPayment(string authority, string status)
    {
        if (status != "OK")
        {
            return new PaymentVerificationResultDto
            {
                IsSuccess = false,
                Message = "پرداخت توسط کاربر لغو شد."
            };
        }

        long amount;
        lock (_pendingPayments)
        {
            if (!_pendingPayments.TryGetValue(authority, out amount))
            {
                return new PaymentVerificationResultDto
                {
                    IsSuccess = false,
                    Message = "اطلاعات پرداخت یافت نشد. ممکن است زمان جلسه منقضی شده باشد."
                };
            }
        }

        var isDevelopment = _options.IsDevelopment;
        var apiUrl = isDevelopment
            ? "https://sandbox.zarinpal.com/pg/v4/payment/verify.json"
            : "https://api.zarinpal.com/pg/v4/payment/verify.json";

        var payload = new
        {
            merchant_id = _options.MerchantId,
            amount = amount,
            authority = authority
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);
            var content = await response.Content.ReadAsStringAsync();

            if (!IsValidJson(content))
            {
                return new PaymentVerificationResultDto
                {
                    IsSuccess = false,
                    Message = "پاسخ نامعتبر از درگاه هنگام تایید پرداخت."
                };
            }

            var result = JsonSerializer.Deserialize<ZarinpalVerifyResponse>(content);

            if (result?.Data != null && (result.Data.Code == 100 || result.Data.Code == 101))
            {
                lock (_pendingPayments)
                {
                    _pendingPayments.Remove(authority);
                }

                return new PaymentVerificationResultDto
                {
                    IsSuccess = true,
                    RefId = result.Data.RefId,
                    Message = $"پرداخت موفق. کد پیگیری: {result.Data.RefId}"
                };
            }

            return new PaymentVerificationResultDto
            {
                IsSuccess = false,
                Message = $"پرداخت ناموفق. کد خطا: {result?.Data?.Code}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment.");
            return new PaymentVerificationResultDto
            {
                IsSuccess = false,
                Message = $"خطا در تایید پرداخت: {ex.Message}"
            };
        }
    }

    private bool IsValidJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;
        content = content.Trim();
        if ((content.StartsWith("{") && content.EndsWith("}")) ||
            (content.StartsWith("[") && content.EndsWith("]")))
        {
            try
            {
                JsonDocument.Parse(content);
                return true;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    // کلاس‌های کمکی (بدون تغییر)
    private class ZarinpalRequestResponse
    {
        public ZarinpalRequestData? Data { get; set; }
        public ZarinpalError? Error { get; set; }
    }

    private class ZarinpalRequestData
    {
        public int Code { get; set; }
        public string Authority { get; set; } = string.Empty;
    }

    private class ZarinpalVerifyResponse
    {
        public ZarinpalVerifyData? Data { get; set; }
        public ZarinpalError? Error { get; set; }
    }

    private class ZarinpalVerifyData
    {
        public int Code { get; set; }
        public long RefId { get; set; }
    }

    private class ZarinpalError
    {
        public string Message { get; set; } = string.Empty;
    }
}