using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class MeterReadingRepository : GenericRepository<MeterReading>, IMeterReadingRepository
    {
        public MeterReadingRepository(AppDbContext context) : base(context)
        {
        }

        // 3.1. Lấy chỉ số điện nước tháng gần nhất trước đó (Để làm mốc số cũ tính tiền)
        public async Task<MeterReading?> GetPreviousReadingAsync(int roomId, int currentMonth, int currentYear)
        {
            return await _context.MeterReadings
                .Where(m => m.RoomId == roomId)
                .Where(m => m.Year < currentYear || (m.Year == currentYear && m.Month < currentMonth))
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .FirstOrDefaultAsync();
        }

        // 3.2. Lấy toàn bộ chỉ số điện nước của 1 dãy trọ trong 1 tháng cụ thể
        public async Task<List<MeterReading>> GetReadingsByHouseAndMonthAsync(int houseId, int month, int year)
        {
            return await _context.MeterReadings
                .Include(m => m.Room)
                .Where(m => m.Room != null 
                         && m.Room.HouseId == houseId 
                         && m.Month == month 
                         && m.Year == year)
                .OrderBy(m => m.Room!.RoomNumber)
                .AsNoTracking()
                .ToListAsync();
        }

        // Kiểm tra xem phòng này trong tháng/năm này đã được ghi chỉ số chưa
        public async Task<MeterReading?> GetReadingByRoomAndMonthAsync(int roomId, int month, int year)
        {
            return await _context.MeterReadings
                .FirstOrDefaultAsync(m => m.RoomId == roomId 
                                       && m.Month == month 
                                       && m.Year == year);
        }
    }
}
