using System;
using System.Collections.Generic;
using System.Text;

// Shared/Models/PaymentModels.cs
namespace NovinApp.Shared;

public class PaymentRequestDto
{
    public int PackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public long Amount { get; set; } // به ریال
    public string Description { get; set; } = string.Empty;
}

public class PaymentResponseDto
{
    public bool IsSuccess { get; set; }
    public string Authority { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class PaymentVerificationDto
{
    public string Authority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class PaymentVerificationResultDto
{
    public bool IsSuccess { get; set; }
    public long RefId { get; set; }
    public string Message { get; set; } = string.Empty;
}
public class ZarinPalOptions
{
    public string MerchantId { get; set; } = string.Empty;
    public bool IsDevelopment { get; set; } = true;
}