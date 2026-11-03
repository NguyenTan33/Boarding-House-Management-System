using Application.DTOs.Auth;
using Domain.Entities;

namespace Application.Interfaces.Auth
{
    public interface ITokenService
    {
        // Sinh JWT Access Token từ thông tin Account
        Task<string> GenerateAccessTokenAsync(Account account);

        // Sinh chuỗi Refresh Token ngẫu nhiên
        string GenerateRefreshToken();

        // Xoay vòng Refresh Token (Token Rotation)
        Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
    }
}
