using Domain.Entities;
using Domain.Enum;

namespace Application.Interfaces.Repositories
{
    public interface IInvoiceRepository : IGenericRepository<Invoice>
    {
        Task<List<Room>> GetRoomsPendingInvoiceAsync(int houseId, int month, int year);
        Task<List<Invoice>> GetInvoicesByStatusAsync(int houseId, int? month, int? year, InvoiceStatus? status);
        Task<Invoice?> GetInvoiceDetailsAsync(int invoiceId);
        Task<Invoice?> GetInvoiceByCodeAsync(string invoiceCode);
    }
}
