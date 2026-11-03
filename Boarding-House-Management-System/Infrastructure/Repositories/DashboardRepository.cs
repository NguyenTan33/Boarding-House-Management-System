using Application.DTOs.Dashboard;
using Application.Interfaces.Repositories;
using Domain.Enum;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        // 5.1. Thống kê tổng doanh thu đã thu vs tổng nợ chưa thu trong tháng
        public async Task<(decimal TotalCollected, decimal TotalUnpaid)> GetRevenueSummaryAsync(int houseId, int month, int year)
        {
            var invoicesInMonth = _context.Invoices
                .Where(i => i.Room != null 
                         && i.Room.HouseId == houseId 
                         && i.Month == month 
                         && i.Year == year 
                         && i.Status != InvoiceStatus.Cancelled);

            // Cho SQL Server tự tính SUM dưới database, trả về 1 con số duy nhất:
            decimal totalCollected = await invoicesInMonth
                .Where(i => i.Status == InvoiceStatus.Paid)
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;

            decimal totalUnpaid = await invoicesInMonth
                .Where(i => i.Status == InvoiceStatus.Unpaid || i.Status == InvoiceStatus.Overdue)
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;

            return (totalCollected, totalUnpaid);
        }

        // 5.2. Thống kê tình trạng phòng trọ (Bao nhiêu phòng trống, bao nhiêu phòng đang ở)
        public async Task<Dictionary<RoomStatus, int>> GetRoomStatusCountsAsync(int houseId)
        {
            return await _context.Rooms
                .Where(r => r.HouseId == houseId && r.IsActive)
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        // 5.3. Thống kê biểu đồ doanh thu 12 tháng trong năm (Dùng cho biểu đồ Chart.js ở Frontend)
        public async Task<List<MonthlyRevenueDto>> GetYearlyRevenueChartAsync(int houseId, int year)
        {
            return await _context.Invoices
                .Where(i => i.Room != null 
                         && i.Room.HouseId == houseId 
                         && i.Year == year 
                         && i.Status == InvoiceStatus.Paid)
                .GroupBy(i => i.Month)
                .Select(g => new MonthlyRevenueDto
                {
                    Month = g.Key,
                    TotalRevenue = g.Sum(i => i.TotalAmount)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();
        }
    }
}
