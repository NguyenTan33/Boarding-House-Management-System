using System.Security.Claims;
using Application.DTOs.Houses;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Landlord,Admin")]
    [EnableRateLimiting("StandardPolicy")]
    public class HousesController : ControllerBase
    {
        private readonly IHouseService _houseService;

        public HousesController(IHouseService houseService)
        {
            _houseService = houseService;
        }

        // 1. GET /api/houses: Danh sách các dãy trọ của chủ trọ (kèm số lượng phòng)
        [HttpGet]
        public async Task<IActionResult> GetHouses()
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool isAdmin = User.IsInRole("Admin");

            // Nếu là Admin thì xem được toàn bộ, nếu là Landlord chỉ xem các dãy do mình sở hữu
            var houses = await _houseService.GetHousesAsync(isAdmin ? null : userId);
            return Ok(houses);
        }

        // 2. GET /api/houses/{id}: Chi tiết 1 dãy trọ kèm đơn giá dịch vụ
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetHouseById(int id)
        {
            var house = await _houseService.GetHouseByIdAsync(id);
            return Ok(house);
        }

        // 3. POST /api/houses: Tạo dãy trọ mới
        [HttpPost]
        public async Task<IActionResult> CreateHouse([FromBody] CreateHouseRequest request)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int houseId = await _houseService.CreateHouseAsync(request, userId);
            return StatusCode(StatusCodes.Status201Created, new { houseId, message = "Tạo dãy trọ thành công." });
        }

        // 4. PUT /api/houses/{id}: Cập nhật thông tin dãy trọ và biểu giá dịch vụ
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateHouse(int id, [FromBody] UpdateHouseRequest request)
        {
            await _houseService.UpdateHouseAsync(id, request);
            return Ok(new { message = "Cập nhật thông tin dãy trọ thành công." });
        }

        // 5. DELETE /api/houses/{id}: Xóa dãy trọ (chỉ cho phép nếu không có phòng nào)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHouse(int id)
        {
            await _houseService.DeleteHouseAsync(id);
            return NoContent();
        }
    }
}
