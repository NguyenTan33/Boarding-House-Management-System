using Application.DTOs.Payments;

namespace Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<VietQrResponse> GenerateVietQrAsync(int invoiceId);
        Task<PaymentResponse> ConfirmCashPaymentAsync(CashPaymentRequest request, string? confirmedByAccountId = null);
        Task<SepayWebhookResultResponse> ProcessSepayWebhookAsync(SepayWebhookRequest request);
    }
}
