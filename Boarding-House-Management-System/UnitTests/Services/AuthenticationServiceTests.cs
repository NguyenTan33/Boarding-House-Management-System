using Application.DTOs.Auth;
using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UnitTests.Helpers;

namespace UnitTests.Services
{
    public class AuthenticationServiceTests
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Account> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly PhoneService _phoneService;
        private readonly HashService _hashService;
        private readonly TokenService _tokenService;
        private readonly AuthenticationService _authService;

        public AuthenticationServiceTests()
        {
            var deps = TestFixture.CreateIdentityDependencies();
            _context = deps.context;
            _userManager = deps.userManager;
            _roleManager = deps.roleManager;
            _configuration = deps.configuration;

            _phoneService = new PhoneService();
            _hashService = new HashService();
            _tokenService = new TokenService(_userManager, _configuration, _hashService, _context);

            _authService = new AuthenticationService(
                _userManager,
                _roleManager,
                _phoneService,
                _tokenService,
                _hashService,
                _context,
                _configuration);
        }

        #region RegisterAsync Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task RegisterAsync_WithEmptyFullName_ShouldThrowValidationException(string? fullName)
        {
            var request = new RegisterRequest
            {
                FullName = fullName!,
                PhoneNumber = "0912345678",
                CitizenId = "123456789012",
                Password = "Password123"
            };

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _authService.RegisterAsync(request));
            Assert.Equal("Họ và tên không được để trống.", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task RegisterAsync_WithEmptyPhoneNumber_ShouldThrowValidationException(string? phone)
        {
            var request = new RegisterRequest
            {
                FullName = "Nguyen Van A",
                PhoneNumber = phone!,
                CitizenId = "123456789012",
                Password = "Password123"
            };

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _authService.RegisterAsync(request));
            Assert.Equal("Số điện thoại không được để trống.", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task RegisterAsync_WithEmptyCitizenId_ShouldThrowValidationException(string? citizenId)
        {
            var request = new RegisterRequest
            {
                FullName = "Nguyen Van A",
                PhoneNumber = "0912345678",
                CitizenId = citizenId!,
                Password = "Password123"
            };

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _authService.RegisterAsync(request));
            Assert.Equal("Số CCCD/CMND không được để trống.", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("12345")] // Less than 6 characters
        public async Task RegisterAsync_WithShortPassword_ShouldThrowValidationException(string? password)
        {
            var request = new RegisterRequest
            {
                FullName = "Nguyen Van A",
                PhoneNumber = "0912345678",
                CitizenId = "123456789012",
                Password = password!
            };

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _authService.RegisterAsync(request));
            Assert.Equal("Mật khẩu phải có độ dài tối thiểu từ 6 ký tự trở lên.", ex.Message);
        }

        [Theory]
        [InlineData("123456")]
        [InlineData("02412345678")]
        [InlineData("09123")]
        public async Task RegisterAsync_WithInvalidPhoneNumber_ShouldThrowValidationException(string phone)
        {
            var request = new RegisterRequest
            {
                FullName = "Nguyen Van A",
                PhoneNumber = phone,
                CitizenId = "123456789012",
                Password = "Password123"
            };

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _authService.RegisterAsync(request));
            Assert.Equal("Định dạng số điện thoại không hợp lệ (vd: 0912345678).", ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_WhenPhoneAlreadyRegistered_ShouldThrowConflictException()
        {
            // Arrange
            var existingAccount = new Account("0912345678", "old@test.com", "Nguyen Van Old", "111111111111", "+84912345678");
            await _userManager.CreateAsync(existingAccount, "Password123");

            var request = new RegisterRequest
            {
                FullName = "Nguyen Van New",
                PhoneNumber = "0912345678",
                CitizenId = "222222222222",
                Password = "Password123"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ConflictException>(() => _authService.RegisterAsync(request));
            Assert.Equal("Số điện thoại này đã được đăng ký trong hệ thống.", ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_WhenCitizenIdAlreadyRegistered_ShouldThrowConflictException()
        {
            // Arrange
            var existingAccount = new Account("0911111111", "old@test.com", "Nguyen Van Old", "123456789012", "+84911111111");
            await _userManager.CreateAsync(existingAccount, "Password123");

            var request = new RegisterRequest
            {
                FullName = "Nguyen Van New",
                PhoneNumber = "0922222222",
                CitizenId = "123456789012",
                Password = "Password123"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ConflictException>(() => _authService.RegisterAsync(request));
            Assert.Equal("Số CCCD/CMND này đã được sử dụng.", ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_WithTenantRole_ShouldSucceedAndAssignTenantRole()
        {
            // Arrange
            var request = new RegisterRequest
            {
                FullName = "Tran Thi B",
                PhoneNumber = "0987654321",
                CitizenId = "987654321098",
                Password = "Password123",
                Email = "tenant@test.com",
                Role = "Tenant"
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.UserId));
            Assert.Equal("Tran Thi B", result.FullName);
            Assert.Equal("+84987654321", result.PhoneNumber);
            Assert.Contains("Khách thuê", result.Message);

            var createdUser = await _userManager.FindByIdAsync(result.UserId);
            Assert.NotNull(createdUser);
            Assert.True(await _userManager.IsInRoleAsync(createdUser, "Tenant"));
        }

        [Fact]
        public async Task RegisterAsync_WithLandlordRole_ShouldSucceedAndAssignLandlordRole()
        {
            // Arrange
            var request = new RegisterRequest
            {
                FullName = "Chủ Trọ A",
                PhoneNumber = "0933333333",
                CitizenId = "333333333333",
                Password = "Password123",
                Role = "Landlord"
            };

            // Act
            var result = await _authService.RegisterAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Chủ trọ", result.Message);

            var createdUser = await _userManager.FindByIdAsync(result.UserId);
            Assert.NotNull(createdUser);
            Assert.True(await _userManager.IsInRoleAsync(createdUser, "Landlord"));
            Assert.Equal("84933333333@boardinghouse.local", createdUser.Email);
        }

        #endregion

        #region LoginAsync Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoginAsync_WithEmptyPhone_ShouldThrowValidationException(string? phone)
        {
            var request = new LoginRequest { PhoneNumber = phone!, Password = "Password123" };
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _authService.LoginAsync(request));
            Assert.Equal("Số điện thoại không được để trống.", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoginAsync_WithEmptyPassword_ShouldThrowValidationException(string? password)
        {
            var request = new LoginRequest { PhoneNumber = "0912345678", Password = password! };
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _authService.LoginAsync(request));
            Assert.Equal("Mật khẩu không được để trống.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_WhenUserNotFound_ShouldThrowBadRequestException()
        {
            var request = new LoginRequest { PhoneNumber = "0912345678", Password = "Password123" };
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _authService.LoginAsync(request));
            Assert.Equal("Số điện thoại hoặc mật khẩu không chính xác.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_WhenAccountIsDeactivated_ShouldThrowForbiddenException()
        {
            // Arrange
            var account = new Account("+84912345678", "user@test.com", "Nguyen A", "123456789", "+84912345678");
            account.Deactivate();
            await _userManager.CreateAsync(account, "Password123");

            var request = new LoginRequest { PhoneNumber = "0912345678", Password = "Password123" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ForbiddenException>(() => _authService.LoginAsync(request));
            Assert.Equal("Tài khoản của bạn đã bị tạm khóa. Vui lòng liên hệ quản trị viên.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIsWrong_ShouldThrowBadRequestException()
        {
            // Arrange
            var account = new Account("+84912345678", "user@test.com", "Nguyen A", "123456789", "+84912345678");
            await _userManager.CreateAsync(account, "CorrectPassword123");

            var request = new LoginRequest { PhoneNumber = "0912345678", Password = "WrongPassword999" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _authService.LoginAsync(request));
            Assert.Equal("Số điện thoại hoặc mật khẩu không chính xác.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnTokensAndSaveRefreshToken()
        {
            // Arrange
            var account = new Account("+84912345678", "user@test.com", "Nguyen A", "123456789", "+84912345678");
            await _userManager.CreateAsync(account, "Password123");
            await _roleManager.CreateAsync(new IdentityRole("Tenant"));
            await _userManager.AddToRoleAsync(account, "Tenant");

            var request = new LoginRequest { PhoneNumber = "0912345678", Password = "Password123" };

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(account.Id, result.UserId);
            Assert.Equal("Nguyen A", result.FullName);
            Assert.Equal("+84912345678", result.PhoneNumber);
            Assert.Contains("Tenant", result.Roles);
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
            Assert.True(result.ExpiresAt > DateTime.UtcNow);

            // Verify RefreshToken was stored in DB
            var secret = _configuration["RefreshToken:Secret"]!;
            var tokenHash = _hashService.Hash(result.RefreshToken, secret);
            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
            Assert.NotNull(storedToken);
            Assert.Equal(account.Id, storedToken.AccountId);
            Assert.False(storedToken.IsRevoked);
        }

        #endregion

        #region LogoutAsync Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LogoutAsync_WithEmptyToken_ShouldThrowValidationException(string? token)
        {
            var request = new LogoutRequest { RefreshToken = token! };
            var ex = await Assert.ThrowsAsync<ValidationException>(() => _authService.LogoutAsync(request));
            Assert.Equal("Refresh Token không được để trống.", ex.Message);
        }

        [Fact]
        public async Task LogoutAsync_WithInvalidToken_ShouldThrowBadRequestException()
        {
            var request = new LogoutRequest { RefreshToken = "non_existent_token" };
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => _authService.LogoutAsync(request));
            Assert.Equal("Refresh Token không hợp lệ.", ex.Message);
        }

        [Fact]
        public async Task LogoutAsync_WhenAlreadyRevoked_ShouldReturnWithoutError()
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
                RevokedAt = DateTime.UtcNow.AddMinutes(-5)
            });
            await _context.SaveChangesAsync();

            var request = new LogoutRequest { RefreshToken = rawToken };

            // Act
            var exception = await Record.ExceptionAsync(() => _authService.LogoutAsync(request));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task LogoutAsync_WithValidToken_ShouldRevokeTokenInDatabase()
        {
            // Arrange
            var rawToken = "active_token_to_logout";
            var secret = _configuration["RefreshToken:Secret"]!;
            var hash = _hashService.Hash(rawToken, secret);

            var tokenEntity = new RefreshToken
            {
                AccountId = "user-1",
                TokenHash = hash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
            _context.RefreshTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();

            var request = new LogoutRequest { RefreshToken = rawToken };

            // Act
            await _authService.LogoutAsync(request);

            // Assert
            var dbToken = await _context.RefreshTokens.FindAsync(tokenEntity.RefreshTokenId);
            Assert.NotNull(dbToken);
            Assert.True(dbToken.IsRevoked);
            Assert.NotNull(dbToken.RevokedAt);
        }

        #endregion

        #region GetCurrentUserProfileAsync Tests

        [Fact]
        public async Task GetCurrentUserProfileAsync_WhenUserNotFound_ShouldThrowNotFoundException()
        {
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _authService.GetCurrentUserProfileAsync("non_existent_id"));

            Assert.Equal("Không tìm thấy thông tin người dùng.", ex.Message);
        }

        [Fact]
        public async Task GetCurrentUserProfileAsync_WhenUserExists_ShouldReturnProfileWithRoles()
        {
            // Arrange
            var account = new Account("+84912345678", "me@test.com", "Nguyen Van Me", "123456789012", "+84912345678", new DateTime(1998, 8, 8), "789 Tran Hung Dao");
            await _userManager.CreateAsync(account, "Password123");
            await _roleManager.CreateAsync(new IdentityRole("Tenant"));
            await _userManager.AddToRoleAsync(account, "Tenant");

            // Act
            var profile = await _authService.GetCurrentUserProfileAsync(account.Id);

            // Assert
            Assert.NotNull(profile);
            Assert.Equal(account.Id, profile.UserId);
            Assert.Equal("Nguyen Van Me", profile.FullName);
            Assert.Equal("+84912345678", profile.PhoneNumber);
            Assert.Equal("me@test.com", profile.Email);
            Assert.Equal("123456789012", profile.CitizenId);
            Assert.Equal("789 Tran Hung Dao", profile.Address);
            Assert.Contains("Tenant", profile.Roles);
        }

        #endregion
    }
}
