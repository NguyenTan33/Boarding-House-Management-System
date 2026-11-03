using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<Account> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IHashService _hashService;
        private readonly AppDbContext _context;

        public TokenService(
            UserManager<Account> userManager,
            IConfiguration configuration,
            IHashService hashService,
            AppDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _hashService = hashService;
            _context = context;
        }

        public async Task<string> GenerateAccessTokenAsync(Account account)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id),
                new Claim(ClaimTypes.Name, account.FullName),
                new Claim(ClaimTypes.MobilePhone, account.PhoneNumber ?? string.Empty),
                new Claim("citizenId", account.CitizenId)
            };

            if (!string.IsNullOrWhiteSpace(account.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, account.Email));
            }

            // Gán các Roles của tài khoản vào Token Claims
            var roles = await _userManager.GetRolesAsync(account);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            var jwtKey = _configuration["Jwt:Key"] 
                ?? "BoardingHouseSecretKeyForJwtAuthenticationSuperSecure2026!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60");

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "BoardingHouseAPI",
                audience: _configuration["Jwt:Audience"] ?? "BoardingHouseClient",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new ValidationException("Refresh Token không được để trống.");

            var refreshTokenSecret = _configuration["RefreshToken:Secret"] 
                ?? "DefaultSecretKeyForRefreshTokenHashingSecure2026!";

            var tokenHash = _hashService.Hash(request.RefreshToken, refreshTokenSecret);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

            if (storedToken == null)
                throw new BadRequestException("Refresh Token không hợp lệ.");

            if (storedToken.IsRevoked)
                throw new BadRequestException("Refresh Token đã bị thu hồi.");

            if (DateTime.UtcNow > storedToken.ExpiresAt)
                throw new BadRequestException("Refresh Token đã hết hạn.");

            var account = await _userManager.FindByIdAsync(storedToken.AccountId);
            if (account == null || !account.IsActive)
                throw new BadRequestException("Tài khoản không tồn tại hoặc đã bị khóa.");

            // Thu hồi token cũ (Token Rotation)
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            // Cấp cặp token mới
            var newAccessToken = await GenerateAccessTokenAsync(account);
            var newRefreshToken = GenerateRefreshToken();
            var newRefreshTokenHash = _hashService.Hash(newRefreshToken, refreshTokenSecret);

            var expireDays = int.Parse(_configuration["RefreshToken:ExpireDays"] ?? "7");

            var newRefreshTokenEntity = new RefreshToken
            {
                AccountId = account.Id,
                TokenHash = newRefreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(expireDays),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60");

            return new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expireMinutes)
            };
        }
    }
}
