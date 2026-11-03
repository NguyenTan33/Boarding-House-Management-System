using Application.DTOs.Auth;

namespace Application.Interfaces.Auth
{
    public interface IAuthenticationService
    {
        // Đăng ký tài khoản mới (Chủ trọ hoặc Khách thuê)
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

        // Đăng nhập bằng Số điện thoại & Mật khẩu
        Task<LoginResponse> LoginAsync(LoginRequest request);

        // Đăng xuất và thu hồi Refresh Token
        Task LogoutAsync(LogoutRequest request);

        // Lấy thông tin cá nhân của người dùng đang đăng nhập
        Task<UserProfileResponse> GetCurrentUserProfileAsync(string userId);
    }
}
