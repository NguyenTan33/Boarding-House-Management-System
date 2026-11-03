using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enum;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ContractRepository : GenericRepository<Contract>, IContractRepository
    {
        public ContractRepository(AppDbContext context) : base(context)
        {
        }

        // 2.1. Lấy hợp đồng đang hoạt động của một phòng cụ thể
        public async Task<Contract?> GetActiveContractByRoomIdAsync(int roomId)
        {
            return await _context.Contracts
                .Include(c => c.Account)
                .Include(c => c.Room)
                .FirstOrDefaultAsync(c => c.RoomId == roomId 
                                       && c.Status == ContractStatus.Active);
        }

        // 2.2. Lấy danh sách các hợp đồng sắp hết hạn trong vòng X ngày tới (Để chủ trọ liên hệ gia hạn)
        public async Task<List<Contract>> GetExpiringContractsAsync(int daysThreshold)
        {
            var targetDate = DateTime.UtcNow.AddDays(daysThreshold);

            return await _context.Contracts
                .Include(c => c.Room)
                .Include(c => c.Account)
                .Where(c => c.Status == ContractStatus.Active 
                         && c.EndDate <= targetDate)
                .OrderBy(c => c.EndDate)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
