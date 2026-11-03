using Application.DTOs.Dashboard;
using Domain.Enum;

namespace Application.Interfaces.Repositories
{
    public interface IDashboardRepository
    {
        Task<(decimal TotalCollected, decimal TotalUnpaid)> GetRevenueSummaryAsync(int houseId, int month, int year);
        Task<Dictionary<RoomStatus, int>> GetRoomStatusCountsAsync(int houseId);
        Task<List<MonthlyRevenueDto>> GetYearlyRevenueChartAsync(int houseId, int year);
    }
}
