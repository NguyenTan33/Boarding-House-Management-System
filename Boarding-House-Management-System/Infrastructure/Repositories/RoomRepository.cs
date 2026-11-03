using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enum;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RoomRepository : GenericRepository<Room>, IRoomRepository
    {
        public RoomRepository(AppDbContext context) : base(context)
        {
        }

        // 1.1. Lấy danh sách phòng có kèm bộ lọc (Filter) và Phân trang (Paging)
        public async Task<List<Room>> GetFilteredRoomsAsync(int houseId, RoomStatus? status, int page, int pageSize)
        {
            var query = _context.Rooms
                .Where(r => r.IsActive && r.HouseId == houseId);

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            return await query
                .OrderBy(r => r.Floor)
                .ThenBy(r => r.RoomNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        // 1.2. Lấy chi tiết 1 phòng kèm thông tin Hợp đồng đang thuê và Khách thuê
        public async Task<Room?> GetRoomWithActiveContractAsync(int roomId)
        {
            return await _context.Rooms
                .Include(r => r.Contracts.Where(c => c.Status == ContractStatus.Active))
                    .ThenInclude(c => c.Account)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoomId == roomId && r.IsActive);
        }

        // 1.3. Kiểm tra số phòng đã tồn tại trong dãy trọ chưa (Tránh tạo trùng số phòng)
        public async Task<bool> IsRoomNumberExistsAsync(int houseId, string roomNumber, int? excludeRoomId = null)
        {
            return await _context.Rooms
                .AnyAsync(r => r.HouseId == houseId 
                            && r.RoomNumber == roomNumber 
                            && (!excludeRoomId.HasValue || r.RoomId != excludeRoomId.Value)
                            && r.IsActive);
        }
    }
}
