using Application.DTOs.Rooms;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("StandardPolicy")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        // 1. GET /api/rooms: Lọc và phân trang phòng trọ
        [HttpGet]
        public async Task<IActionResult> GetRooms([FromQuery] RoomFilterRequest filter)
        {
            var pagedRooms = await _roomService.GetRoomsAsync(filter);
            return Ok(pagedRooms);
        }

        // 2. GET /api/rooms/{id}: Chi tiết phòng kèm hợp đồng đang thuê (nếu có)
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            return Ok(room);
        }

        // 3. POST /api/rooms: Tạo phòng trọ mới
        [HttpPost]
        [Authorize(Roles = "Landlord,Admin")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
        {
            int roomId = await _roomService.CreateRoomAsync(request);
            return StatusCode(StatusCodes.Status201Created, new { roomId, message = "Thêm phòng trọ thành công." });
        }

        // 4. PUT /api/rooms/{id}: Cập nhật thông tin phòng
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Landlord,Admin")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomRequest request)
        {
            await _roomService.UpdateRoomAsync(id, request);
            return NoContent();
        }

        // 5. PATCH /api/rooms/{id}/status: Cập nhật trạng thái phòng (Bảo trì, Trống...)
        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "Landlord,Admin")]
        public async Task<IActionResult> UpdateRoomStatus(int id, [FromBody] UpdateRoomStatusRequest request)
        {
            await _roomService.UpdateRoomStatusAsync(id, request);
            return Ok(new { message = "Cập nhật trạng thái phòng thành công." });
        }

        // 6. DELETE /api/rooms/{id}: Xóa phòng (chặn nếu đang có người ở)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Landlord,Admin")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            await _roomService.DeleteRoomAsync(id);
            return NoContent();
        }
    }
}
