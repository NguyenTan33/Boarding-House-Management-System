using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IHouseRepository : IGenericRepository<House>
    {
        Task<List<House>> GetHousesWithRoomsAsync(string? ownerId = null);
        Task<House?> GetHouseWithRoomsByIdAsync(int houseId);
    }
}
