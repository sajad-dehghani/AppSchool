// Client/Services/IPaymentService.cs
using NovinApp.Shared;
using System.Net.Http.Json;

namespace NovinApp.Client.Services;

public interface IPaymentService
{
    Task<PaymentResponseDto> RequestPayment(PaymentRequestDto request);
}
