namespace Application.DTOs.Auth
{
    // Yêu cầu đăng ký tài khoản
    public record RegisterRequest
    {
        public string FullName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string CitizenId { get; init; } = string.Empty; // Số CCCD / CMND
        public string Password { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string Role { get; init; } = "Tenant";          // "Tenant" (Khách thuê) hoặc "Landlord" (Chủ trọ)
    }

    // Kết quả đăng ký thành công
    public record RegisterResponse
    {
        public string UserId { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
    }

    // Yêu cầu đăng nhập (bằng SĐT hoặc Email)
    public record LoginRequest
    {
        public string PhoneNumber { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }

    // Kết quả đăng nhập
    public record LoginResponse
    {
        public string UserId { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public List<string> Roles { get; init; } = new();
        public string AccessToken { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; }
    }

    // Yêu cầu làm mới Access Token
    public record RefreshTokenRequest
    {
        public string RefreshToken { get; init; } = string.Empty;
    }

    // Kết quả cấp Access Token mới
    public record RefreshTokenResponse
    {
        public string AccessToken { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; }
    }

    // Yêu cầu đăng xuất
    public record LogoutRequest
    {
        public string RefreshToken { get; init; } = string.Empty;
    }

    // Thông tin người dùng đang đăng nhập
    public record UserProfileResponse
    {
        public string UserId { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string CitizenId { get; init; } = string.Empty;
        public DateTime? DateOfBirth { get; init; }
        public string? Gender { get; init; }
        public string? Address { get; init; }
        public string? AvatarUrl { get; init; }
        public List<string> Roles { get; init; } = new();
    }
}
