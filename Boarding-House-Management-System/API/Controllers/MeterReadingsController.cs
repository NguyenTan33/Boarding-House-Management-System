using System.Security.Claims;
using Application.DTOs.MeterReadings;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    [Route("api/meter-readings")]
    [ApiController]
    [Authorize(Roles = "Landlord,Admin")]
    [EnableRateLimiting("StandardPolicy")]
    public class MeterReadingsController : ControllerBase
    {
        private readonly IMeterReadingService _meterReadingService;

        public MeterReadingsController(IMeterReadingService meterReadingService)
        {
            _meterReadingService = meterReadingService;
        }

        // 1. GET /api/meter-readings: Bảng theo dõi số điện nước của tất cả các phòng trong tháng
        [HttpGet]
        public async Task<IActionResult> GetReadingsByHouseAndMonth(
            [FromQuery] int houseId,
            [FromQuery] int month,
            [FromQuery] int year)
        {
            var readings = await _meterReadingService.GetReadingsByHouseAndMonthAsync(houseId, month, year);
            return Ok(readings);
        }

        // 2. POST /api/meter-readings: Ghi nhận chỉ số điện nước cho 1 phòng
        [HttpPost]
        public async Task<IActionResult> RecordMeterReading([FromBody] RecordMeterReadingRequest request)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var response = await _meterReadingService.RecordMeterReadingAsync(request, userId);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        // 3. POST /api/meter-readings/bulk: Ghi nhận chỉ số hàng loạt phòng cùng lúc (RateLimit: HeavyPolicy)
        [HttpPost("bulk")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> BulkRecordMeterReadings([FromBody] BulkMeterReadingRequest request)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var response = await _meterReadingService.BulkRecordMeterReadingsAsync(request, userId);
            return Ok(response);
        }

        // 4. GET /api/meter-readings/rooms/{roomId}/previous: Lấy chỉ số tháng trước của phòng để tự động điền số cũ
        [HttpGet("rooms/{roomId:int}/previous")]
        public async Task<IActionResult> GetPreviousReading(
            int roomId, 
            [FromQuery] int? month = null, 
            [FromQuery] int? year = null)
        {
            int targetMonth = month ?? DateTime.UtcNow.Month;
            int targetYear = year ?? DateTime.UtcNow.Year;

            var previous = await _meterReadingService.GetPreviousReadingAsync(roomId, targetMonth, targetYear);
            return Ok(previous);
        }
    }
}
