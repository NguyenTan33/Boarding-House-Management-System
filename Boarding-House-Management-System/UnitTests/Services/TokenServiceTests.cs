using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.DTOs.Auth;
using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using UnitTests.Helpers;

namespace UnitTests.Services
{
    public class TokenServiceTests
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Account> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly HashService _hashService;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            var deps = TestFixture.CreateIdentityDependencies();
            _context = deps.context;
            _userManager = deps.userManager;
            _roleManager = deps.roleManager;
            _configuration = deps.configuration;
            _hashService = new HashService();
            _tokenService = new TokenService(_userManager, _configuration, _hashService, _context);
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnRandomBase64String()
        {
            // Act
            var token1 = _tokenService.GenerateRefreshToken();
            var token2 = _tokenService.GenerateRefreshToken();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token1));
            Assert.False(string.IsNullOrWhiteSpace(token2));
            Assert.NotEqual(token1, token2);
            // Verify valid Base64
            var bytes = Convert.FromBase64String(token1);
            Assert.Equal(64, bytes.Length);
        }

        [Fact]
        public async Task GenerateAccessTokenAsync_ShouldGenerateValidJwtWithExpectedClaims()
        {
            // Arrange
            var account = new Account("0912345678", "test@domain.com", "Nguyen Van A", "123456789", "0912345678");
            await _userManager.CreateAsync(account, "Password123!");
            await _roleManager.CreateAsync(new IdentityRole("Tenant"));
            await _userManager.AddToRoleAsync(account, "Tenant");

            // Act
            var tokenString = await _tokenService.GenerateAccessTokenAsync(account);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(tokenString));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(tokenString);

            Assert.Equal(_configuration["Jwt:Issuer"], jwt.Issuer);
            Assert.Contains(_configuration["Jwt:Audience"], jwt.Audiences);
            Assert.Equal(account.Id, jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal(account.FullName, jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value);
            Assert.Equal(account.PhoneNumber, jwt.Claims.First(c => c.Type == ClaimTypes.MobilePhone).Value);
            Assert.Equal(account.Email, jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
            Assert.Equal(account.CitizenId, jwt.Claims.First(c => c.Type == "citizenId").Value);
            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Tenant");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task RefreshTokenAsync_WithEmptyToken_ShouldThrowValidationException(string? token)
        {
            // Arrange
            var request = new RefreshTokenRequest { RefreshToken = token! };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                _tokenService.RefreshTokenAsync(request));

            Assert.Equal("Refresh Token không được để trống.", ex.Message);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithNonExistentToken_ShouldThrowBadRequestException()
        {
            // Arrange
            var request = new RefreshTokenRequest { RefreshToken = "non_existent_token_string" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _tokenService.RefreshTokenAsync(request));

            Assert.Equal("Refresh Token không hợp lệ.", ex.Message);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithRevokedToken_ShouldThrowBadRequestException()
        {
            // Arrange
            var rawToken = "already_revoked_token";
            var secret = _configuration["RefreshToken:Secret"]!;
            var hash = _hashService.Hash(rawToken, secret);

            _context.RefreshTokens.Add(new RefreshToken
            {
                AccountId = "user-1",
                TokenHash = hash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = true,
                RevokedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var request = new RefreshTokenRequest { RefreshToken = rawToken };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _tokenService.RefreshTokenAsync(request));

            Assert.Equal("Refresh Token đã bị thu hồi.", ex.Message);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithExpiredToken_ShouldThrowBadRequestException()
        {
            // Arrange
            var rawToken = "expired_token_string";
            var secret = _configuration["RefreshToken:Secret"]!;
            var hash = _hashService.Hash(rawToken, secret);

            _context.RefreshTokens.Add(new RefreshToken
            {
                AccountId = "user-1",
                TokenHash = hash,
                ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired yesterday
                IsRevoked = false
            });
            await _context.SaveChangesAsync();

            var request = new RefreshTokenRequest { RefreshToken = rawToken };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _tokenService.RefreshTokenAsync(request));

            Assert.Equal("Refresh Token đã hết hạn.", ex.Message);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenUserNotFoundOrInactive_ShouldThrowBadRequestException()
        {
            // Arrange
            var account = new Account("0912345678", "test@domain.com", "Nguyen Van A", "123456789", "0912345678");
            account.Deactivate();
            await _userManager.CreateAsync(account, "Password123!");

            var rawToken = "valid_token_inactive_user";
            var secret = _configuration["RefreshToken:Secret"]!;
            var hash = _hashService.Hash(rawToken, secret);

            _context.RefreshTokens.Add(new RefreshToken
            {
                AccountId = account.Id,
                TokenHash = hash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });
            await _context.SaveChangesAsync();

            var request = new RefreshTokenRequest { RefreshToken = rawToken };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _tokenService.RefreshTokenAsync(request));

            Assert.Equal("Tài khoản không tồn tại hoặc đã bị khóa.", ex.Message);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ShouldRotateTokenAndReturnNewTokens()
        {
            // Arrange
            var account = new Account("0912345678", "test@domain.com", "Nguyen Van A", "123456789", "0912345678");
            await _userManager.CreateAsync(account, "Password123!");
            await _roleManager.CreateAsync(new IdentityRole("Tenant"));
            await _userManager.AddToRoleAsync(account, "Tenant");

            var rawOldToken = "valid_original_token";
            var secret = _configuration["RefreshToken:Secret"]!;
            var oldHash = _hashService.Hash(rawOldToken, secret);

            var oldTokenEntity = new RefreshToken
            {
                AccountId = account.Id,
                TokenHash = oldHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
            _context.RefreshTokens.Add(oldTokenEntity);
            await _context.SaveChangesAsync();

            var request = new RefreshTokenRequest { RefreshToken = rawOldToken };

            // Act
            var result = await _tokenService.RefreshTokenAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
            Assert.NotEqual(rawOldToken, result.RefreshToken);
            Assert.True(result.ExpiresAt > DateTime.UtcNow);

            // Verify old token is now revoked in DB
            var dbOldToken = await _context.RefreshTokens.FindAsync(oldTokenEntity.RefreshTokenId);
            Assert.NotNull(dbOldToken);
            Assert.True(dbOldToken.IsRevoked);
            Assert.NotNull(dbOldToken.RevokedAt);

            // Verify new token exists in DB
            var newHash = _hashService.Hash(result.RefreshToken, secret);
            var dbNewToken = _context.RefreshTokens.FirstOrDefault(t => t.TokenHash == newHash);
            Assert.NotNull(dbNewToken);
            Assert.False(dbNewToken.IsRevoked);
            Assert.Equal(account.Id, dbNewToken.AccountId);
        }
    }
}
