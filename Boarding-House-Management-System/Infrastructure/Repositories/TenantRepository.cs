using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TenantRepository : GenericRepository<Account>, ITenantRepository
    {
        public TenantRepository(AppDbContext context) : base(context)
        {
        }

        // 2.3. Tìm kiếm khách thuê theo Tên, SĐT hoặc Số CCCD
        public async Task<List<Account>> SearchTenantsAsync(string keyword)
        {
            keyword = keyword.Trim().ToLower();

            return await _dbSet
                .Where(t => t.IsActive && 
                           (t.FullName.ToLower().Contains(keyword) || 
                            (t.PhoneNumber != null && t.PhoneNumber.Contains(keyword)) || 
                            t.CitizenId.Contains(keyword)))
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
