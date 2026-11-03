using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enum;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(AppDbContext context) : base(context)
        {
        }

        // 4.1. Lấy danh sách phòng ĐANG CÓ NGƯỜI Ở nhưng CHƯA ĐƯỢC TẠO HÓA ĐƠN tháng này
        public async Task<List<Room>> GetRoomsPendingInvoiceAsync(int houseId, int month, int year)
        {
            return await _context.Rooms
                .Where(r => r.HouseId == houseId 
                         && r.Status == RoomStatus.Occupied 
                         && r.IsActive)
                .Where(r => !_context.Invoices.Any(i => i.RoomId == r.RoomId 
                                                     && i.Month == month 
                                                     && i.Year == year 
                                                     && i.Status != InvoiceStatus.Cancelled))
                .Include(r => r.MeterReadings.Where(m => m.Month == month && m.Year == year))
                .ToListAsync();
        }

        // 4.2. Lấy danh sách hóa đơn theo trạng thái (Chưa thanh toán / Đã thanh toán / Quá hạn)
        public async Task<List<Invoice>> GetInvoicesByStatusAsync(int houseId, int? month, int? year, InvoiceStatus? status)
        {
            var query = _context.Invoices
                .Include(i => i.Room)
                .Where(i => i.Room != null && i.Room.HouseId == houseId);

            if (month.HasValue && year.HasValue)
                query = query.Where(i => i.Month == month.Value && i.Year == year.Value);

            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);

            return await query
                .OrderByDescending(i => i.IssueDate)
                .AsNoTracking()
                .ToListAsync();
        }

        // 4.3. Lấy chi tiết 1 hóa đơn kèm thông tin Phòng, Dãy trọ và Lịch sử thanh toán
        public async Task<Invoice?> GetInvoiceDetailsAsync(int invoiceId)
        {
            return await _context.Invoices
                .Include(i => i.Room)
                    .ThenInclude(r => r!.House)
                .Include(i => i.Account)
                .Include(i => i.Contract)
                .Include(i => i.MeterReading)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
        }

        // Tra cứu hóa đơn theo mã code (vd: HDT-202611-P101)
        public async Task<Invoice?> GetInvoiceByCodeAsync(string invoiceCode)
        {
            return await _context.Invoices
                .Include(i => i.Room)
                    .ThenInclude(r => r!.House)
                .Include(i => i.Account)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceCode == invoiceCode);
        }
    }
}
