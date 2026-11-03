using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<List<Payment>> GetPaymentsByInvoiceIdAsync(int invoiceId);
        Task<Payment?> GetPaymentByCodeAsync(string paymentCode);
        Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId);
    }
}
