namespace Application.Payment.DTOs;

public class PaymentIntentResult
{
    public string? PaymentIntentId { get; set; }
    public string? ClientSecret { get; set; }
}
