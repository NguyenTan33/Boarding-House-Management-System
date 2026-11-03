using Domain.Entities;
using Domain.Enum;

namespace Application.Interfaces.Repositories
{
    public interface IRoomRepository : IGenericRepository<Room>
    {
        Task<List<Room>> GetFilteredRoomsAsync(int houseId, RoomStatus? status, int page, int pageSize);
        Task<Room?> GetRoomWithActiveContractAsync(int roomId);
        Task<bool> IsRoomNumberExistsAsync(int houseId, string roomNumber, int? excludeRoomId = null);
    }
}
