// Client/Services/PaymentService.cs
using NovinApp.Shared;
using NovinApp.Client.Services;
using NovinApp.Shared;
using System.Net.Http.Json;

namespace NovinApp.Client.Services;

public class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;

    public PaymentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaymentResponseDto> RequestPayment(PaymentRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/payment/request", request);
        var result = await response.Content.ReadFromJsonAsync<PaymentResponseDto>();
        return result ?? new PaymentResponseDto { IsSuccess = false, Message = "خطا در ارتباط با سرور" };
    }
}