using Application.DTOs.Dashboard;
using Application.Interfaces.Services;
using API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Controllers
{
    public class DashboardControllerTests
    {
        private readonly Mock<IDashboardService> _mockDashboardService;
        private readonly DashboardController _controller;

        public DashboardControllerTests()
        {
            _mockDashboardService = new Mock<IDashboardService>();
            _controller = new DashboardController(_mockDashboardService.Object);
        }

        [Fact]
        public async Task GetSummary_ShouldReturnOkWithDashboardSummary()
        {
            // Arrange
            var houseId = 1;
            int? month = 11;
            int? year = 2026;
            var summary = new DashboardSummaryResponse
            {
                HouseId = houseId,
                Month = 11,
                Year = 2026,
                ExpectedRevenue = 20000000,
                ActualRevenueCollected = 15000000,
                TotalUnpaidDebt = 5000000,
                RoomStatus = new RoomStatusBreakdownDto
                {
                    TotalRooms = 10,
                    OccupiedRooms = 8,
                    AvailableRooms = 2
                }
            };
            _mockDashboardService.Setup(s => s.GetDashboardSummaryAsync(houseId, month, year)).ReturnsAsync(summary);

            // Act
            var result = await _controller.GetSummary(houseId, month, year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsType<DashboardSummaryResponse>(okResult.Value);
            Assert.Equal(20000000, actual.ExpectedRevenue);
            Assert.Equal(8, actual.RoomStatus.OccupiedRooms);
        }

        [Fact]
        public async Task GetRevenueChart_ShouldReturnOkWithYearlyChart()
        {
            // Arrange
            var houseId = 1;
            var year = 2026;
            var chartList = new List<MonthlyRevenueDto>
            {
                new() { Month = 1, TotalRevenue = 10000000 },
                new() { Month = 2, TotalRevenue = 12000000 }
            };
            _mockDashboardService.Setup(s => s.GetYearlyRevenueChartAsync(houseId, year)).ReturnsAsync(chartList);

            // Act
            var result = await _controller.GetRevenueChart(houseId, year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsAssignableFrom<IEnumerable<MonthlyRevenueDto>>(okResult.Value);
            Assert.Equal(2, actual.Count());
        }
    }
}
