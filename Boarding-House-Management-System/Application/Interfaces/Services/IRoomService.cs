using Application.DTOs.Common;
using Application.DTOs.Rooms;

namespace Application.Interfaces.Services
{
    public interface IRoomService
    {
        Task<PagedResult<RoomResponse>> GetRoomsAsync(RoomFilterRequest filter);
        Task<RoomDetailResponse> GetRoomByIdAsync(int id);
        Task<int> CreateRoomAsync(CreateRoomRequest request);
        Task UpdateRoomAsync(int id, UpdateRoomRequest request);
        Task UpdateRoomStatusAsync(int id, UpdateRoomStatusRequest request);
        Task DeleteRoomAsync(int id);
    }
}
