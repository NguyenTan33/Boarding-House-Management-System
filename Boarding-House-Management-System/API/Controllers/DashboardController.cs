using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Landlord,Admin")]
    [EnableRateLimiting("HeavyPolicy")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // 1. GET /api/dashboard/summary: Thống kê doanh thu dự kiến, thực thu, nợ chưa thu và tỷ lệ phòng
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] int houseId,
            [FromQuery] int? month = null,
            [FromQuery] int? year = null)
        {
            var summary = await _dashboardService.GetDashboardSummaryAsync(houseId, month, year);
            return Ok(summary);
        }

        // 2. GET /api/dashboard/revenue-chart: Biểu đồ doanh thu 12 tháng trong năm
        [HttpGet("revenue-chart")]
        public async Task<IActionResult> GetRevenueChart(
            [FromQuery] int houseId,
            [FromQuery] int year)
        {
            var chart = await _dashboardService.GetYearlyRevenueChartAsync(houseId, year);
            return Ok(chart);
        }
    }
}
