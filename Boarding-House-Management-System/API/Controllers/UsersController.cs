using System.Security.Claims;
using Application.DTOs.Users;
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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        #region 1. CHỨC NĂNG CÁ NHÂN (USER & ADMIN)

        // 1.1. GET /api/users/profile: Lấy thông tin tài khoản của chính mình (kèm thông tin phòng đang ở nếu là khách thuê)
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profile = await _userService.GetCurrentUserProfileAsync(userId);
            return Ok(profile);
        }

        // 1.2. PUT /api/users/profile: Chỉnh sửa thông tin cá nhân (Họ tên, CCCD, ngày sinh, địa chỉ, ảnh đại diện)
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _userService.UpdateCurrentUserProfileAsync(userId, request);
            return Ok(new { message = "Cập nhật thông tin cá nhân thành công." });
        }

        // 1.3. PUT /api/users/change-password: Đổi mật khẩu cá nhân
        [HttpPut("change-password")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _userService.ChangePasswordAsync(userId, request);
            return Ok(new { message = "Đổi mật khẩu thành công. Vui lòng sử dụng mật khẩu mới cho lần đăng nhập tiếp theo." });
        }

        #endregion

        #region 2. CHỨC NĂNG QUẢN LÝ NGƯỜI DÙNG DÀNH RIÊNG CHO ADMIN

        // 2.1. GET /api/users: Xem danh sách toàn bộ người dùng, lọc theo vai trò, từ khóa, dãy trọ; hiển thị đang ở phòng nào trọ nào ngày vào
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers([FromQuery] UserFilterRequest filter)
        {
            var pagedUsers = await _userService.GetUsersAsync(filter);
            return Ok(pagedUsers);
        }

        // 2.2. GET /api/users/{id}: Xem chi tiết thông tin 1 người dùng kèm toàn bộ lịch sử thuê phòng
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserDetail(string id)
        {
            var userDetail = await _userService.GetUserDetailAsync(id);
            return Ok(userDetail);
        }

        // 2.3. POST /api/users/{id}/lock: Khóa tài khoản người dùng vĩnh viễn và kick out ngay lập tức
        [HttpPost("{id}/lock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LockUserPermanently(string id, [FromBody] LockUserRequest? request)
        {
            string? adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            await _userService.LockUserPermanentlyAsync(id, adminId, request?.Reason);
            return Ok(new { message = "Đã khóa tài khoản vĩnh viễn và thu hồi toàn bộ phiên đăng nhập của người dùng." });
        }

        // 2.4. POST /api/users/{id}/unlock: Mở khóa cho tài khoản người dùng
        [HttpPost("{id}/unlock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnlockUser(string id)
        {
            string? adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            await _userService.UnlockUserAsync(id, adminId);
            return Ok(new { message = "Mở khóa tài khoản thành công." });
        }

        // 2.5. POST /api/users/{id}/reset-password: Admin cấp lại mật khẩu cho người dùng
        [HttpPost("{id}/reset-password")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> ResetUserPassword(string id, [FromBody] AdminResetPasswordRequest request)
        {
            string? adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized();

            await _userService.ResetUserPasswordAsync(id, request, adminId);
            return Ok(new { message = "Cấp lại mật khẩu thành công. Người dùng có thể đăng nhập bằng mật khẩu mới này." });
        }

        #endregion
    }
}
