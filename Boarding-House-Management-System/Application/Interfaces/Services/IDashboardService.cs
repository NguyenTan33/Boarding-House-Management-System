using Application.DTOs.Dashboard;

namespace Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryResponse> GetDashboardSummaryAsync(int houseId, int? month = null, int? year = null);
        Task<List<MonthlyRevenueDto>> GetYearlyRevenueChartAsync(int houseId, int year);
    }
}
