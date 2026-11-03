using Application.DTOs.Houses;

namespace Application.Interfaces.Services
{
    public interface IHouseService
    {
        Task<List<HouseSummaryResponse>> GetHousesAsync(string? ownerId = null);
        Task<HouseDetailResponse> GetHouseByIdAsync(int id);
        Task<int> CreateHouseAsync(CreateHouseRequest request, string? ownerId = null);
        Task UpdateHouseAsync(int id, UpdateHouseRequest request);
        Task DeleteHouseAsync(int id);
    }
}
