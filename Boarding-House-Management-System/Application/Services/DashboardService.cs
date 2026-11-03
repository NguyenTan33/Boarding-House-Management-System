using Application.DTOs.Dashboard;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enum;

namespace Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IHouseRepository _houseRepository;

        public DashboardService(
            IDashboardRepository dashboardRepository,
            IHouseRepository houseRepository)
        {
            _dashboardRepository = dashboardRepository;
            _houseRepository = houseRepository;
        }

        public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(int houseId, int? month = null, int? year = null)
        {
            var house = await _houseRepository.GetByIdAsync(houseId);
            if (house == null || !house.IsActive)
                throw new NotFoundException($"Không tìm thấy dãy trọ với ID {houseId}.");

            int targetMonth = month ?? DateTime.UtcNow.Month;
            int targetYear = year ?? DateTime.UtcNow.Year;

            // 1. Thống kê doanh thu thực tế đã thu vs nợ chưa thu
            var (totalCollected, totalUnpaid) = await _dashboardRepository.GetRevenueSummaryAsync(houseId, targetMonth, targetYear);
            decimal expectedRevenue = totalCollected + totalUnpaid;

            // 2. Thống kê tình trạng phòng
            var statusCounts = await _dashboardRepository.GetRoomStatusCountsAsync(houseId);

            int totalRooms = statusCounts.Values.Sum();
            statusCounts.TryGetValue(RoomStatus.Occupied, out int occupied);
            statusCounts.TryGetValue(RoomStatus.Available, out int available);
            statusCounts.TryGetValue(RoomStatus.UnderMaintenance, out int maintenance);
            statusCounts.TryGetValue(RoomStatus.Reserved, out int reserved);

            return new DashboardSummaryResponse
            {
                HouseId = houseId,
                Month = targetMonth,
                Year = targetYear,
                ExpectedRevenue = expectedRevenue,
                ActualRevenueCollected = totalCollected,
                TotalUnpaidDebt = totalUnpaid,
                RoomStatus = new RoomStatusBreakdownDto
                {
                    TotalRooms = totalRooms,
                    OccupiedRooms = occupied,
                    AvailableRooms = available,
                    MaintenanceRooms = maintenance,
                    ReservedRooms = reserved
                }
            };
        }

        public async Task<List<MonthlyRevenueDto>> GetYearlyRevenueChartAsync(int houseId, int year)
        {
            var house = await _houseRepository.GetByIdAsync(houseId);
            if (house == null || !house.IsActive)
                throw new NotFoundException($"Không tìm thấy dãy trọ với ID {houseId}.");

            return await _dashboardRepository.GetYearlyRevenueChartAsync(houseId, year);
        }
    }
}
