using Application.DTOs.Invoices;

namespace Application.Interfaces.Services
{
    public interface IBillingService
    {
        Task<GenerateInvoicesResultResponse> GenerateMonthlyInvoicesAsync(GenerateMonthlyInvoicesRequest request);
        Task<List<InvoiceResponse>> GetInvoicesAsync(InvoiceFilterRequest filter);
        Task<InvoiceDetailResponse> GetInvoiceByIdAsync(int id);
        Task CancelInvoiceAsync(int id, string? reason = null);
    }
}
