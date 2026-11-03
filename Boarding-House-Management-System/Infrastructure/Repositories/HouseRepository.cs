using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class HouseRepository : GenericRepository<House>, IHouseRepository
    {
        public HouseRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<House>> GetHousesWithRoomsAsync(string? ownerId = null)
        {
            var query = _context.Houses
                .Include(h => h.Rooms.Where(r => r.IsActive))
                .Where(h => h.IsActive);

            if (!string.IsNullOrEmpty(ownerId))
            {
                query = query.Where(h => h.OwnerId == ownerId);
            }

            return await query
                .OrderBy(h => h.HouseName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<House?> GetHouseWithRoomsByIdAsync(int houseId)
        {
            return await _context.Houses
                .Include(h => h.Rooms.Where(r => r.IsActive))
                .FirstOrDefaultAsync(h => h.HouseId == houseId && h.IsActive);
        }
    }
}
