// Server/Controllers/PaymentController.cs
using NovinApp.Server.Services;
using NovinApp.Shared;
using Microsoft.AspNetCore.Mvc;

namespace NovinApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly ZarinpalService _zarinpalService;

    public PaymentController(ZarinpalService zarinpalService)
    {
        _zarinpalService = zarinpalService;
    }

    [HttpPost("request")]
    public async Task<ActionResult<PaymentResponseDto>> RequestPayment(PaymentRequestDto request)
    {
        var result = await _zarinpalService.RequestPayment(request);
        return Ok(result);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string authority, [FromQuery] string status)
    {
        var result = await _zarinpalService.VerifyPayment(authority, status);

        // هدایت به صفحه نتیجه در Blazor WASM
        var frontendUrl = $"{Request.Scheme}://{Request.Host}/payment-result?refId={result.RefId}&isSuccess={result.IsSuccess}&message={Uri.EscapeDataString(result.Message)}";

        return Redirect(frontendUrl);
    }
}