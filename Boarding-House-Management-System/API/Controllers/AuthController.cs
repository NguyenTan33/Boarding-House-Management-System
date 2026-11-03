using System.Security.Claims;
using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("AuthPolicy")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ITokenService _tokenService;

        public AuthController(
            IAuthenticationService authenticationService,
            ITokenService tokenService)
        {
            _authenticationService = authenticationService;
            _tokenService = tokenService;
        }

        // ĐĂNG KÝ TÀI KHOẢN (Khách thuê hoặc Chủ trọ)
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _authenticationService.RegisterAsync(request);
            return Ok(response);
        }

        // ĐĂNG NHẬP (Bằng Số điện thoại & Mật khẩu)
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authenticationService.LoginAsync(request);
            return Ok(response);
        }

        // LÀM MỚI TOKEN (Refresh Token Rotation)
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _tokenService.RefreshTokenAsync(request);
            return Ok(response);
        }

        // ĐĂNG XUẤT (Thu hồi Refresh Token)
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _authenticationService.LogoutAsync(request);
            return Ok(new { message = "Đăng xuất thành công." });
        }

        // LẤY THÔNG TIN TÀI KHOẢN ĐANG ĐĂNG NHẬP
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Không xác định được danh tính người dùng." });

            var profile = await _authenticationService.GetCurrentUserProfileAsync(userId);
            return Ok(profile);
        }
    }
}
