using System.Security.Claims;
using API.Controllers;
using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthenticationService> _mockAuthService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockTokenService = new Mock<ITokenService>();
            _controller = new AuthController(_mockAuthService.Object, _mockTokenService.Object);
        }

        [Fact]
        public async Task Register_WithValidRequest_ShouldReturnOkWithRegisterResponse()
        {
            // Arrange
            var request = new RegisterRequest
            {
                FullName = "Nguyen Van A",
                PhoneNumber = "0912345678",
                CitizenId = "123456789012",
                Password = "Password123",
                Role = "Tenant"
            };

            var expectedResponse = new RegisterResponse
            {
                UserId = "user-123",
                FullName = "Nguyen Van A",
                PhoneNumber = "+84912345678",
                Message = "Đăng ký tài khoản thành công với vai trò Khách thuê."
            };

            _mockAuthService.Setup(s => s.RegisterAsync(request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<RegisterResponse>(okResult.Value);
            Assert.Equal("user-123", response.UserId);
            Assert.Equal(expectedResponse.Message, response.Message);
        }

        [Fact]
        public async Task Login_WithValidRequest_ShouldReturnOkWithLoginResponse()
        {
            // Arrange
            var request = new LoginRequest { PhoneNumber = "0912345678", Password = "Password123" };
            var expectedResponse = new LoginResponse
            {
                UserId = "user-123",
                FullName = "Nguyen Van A",
                PhoneNumber = "+84912345678",
                Roles = new List<string> { "Tenant" },
                AccessToken = "jwt-access-token",
                RefreshToken = "random-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            };

            _mockAuthService.Setup(s => s.LoginAsync(request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponse>(okResult.Value);
            Assert.Equal("jwt-access-token", response.AccessToken);
            Assert.Equal("random-refresh-token", response.RefreshToken);
        }

        [Fact]
        public async Task RefreshToken_WithValidRequest_ShouldReturnOkWithRefreshTokenResponse()
        {
            // Arrange
            var request = new RefreshTokenRequest { RefreshToken = "valid-refresh-token" };
            var expectedResponse = new RefreshTokenResponse
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            };

            _mockTokenService.Setup(s => s.RefreshTokenAsync(request)).ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RefreshToken(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<RefreshTokenResponse>(okResult.Value);
            Assert.Equal("new-access-token", response.AccessToken);
            Assert.Equal("new-refresh-token", response.RefreshToken);
        }

        [Fact]
        public async Task Logout_WithValidRequest_ShouldCallLogoutAsyncAndReturnOkMessage()
        {
            // Arrange
            var request = new LogoutRequest { RefreshToken = "token-to-revoke" };
            _mockAuthService.Setup(s => s.LogoutAsync(request)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            _mockAuthService.Verify(s => s.LogoutAsync(request), Times.Once);
        }

        [Fact]
        public async Task GetCurrentUserProfile_WhenUserIdClaimMissing_ShouldReturnUnauthorized()
        {
            // Arrange
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity()) // No claims
                }
            };
            _controller.ControllerContext = context;

            // Act
            var result = await _controller.GetCurrentUserProfile();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetCurrentUserProfile_WhenAuthenticated_ShouldReturnOkWithProfile()
        {
            // Arrange
            var userId = "user-123";
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var expectedProfile = new UserProfileResponse
            {
                UserId = userId,
                FullName = "Nguyen Van A",
                PhoneNumber = "+84912345678",
                Email = "a@test.com",
                CitizenId = "123456789012",
                Roles = new List<string> { "Tenant" }
            };

            _mockAuthService.Setup(s => s.GetCurrentUserProfileAsync(userId)).ReturnsAsync(expectedProfile);

            // Act
            var result = await _controller.GetCurrentUserProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var profile = Assert.IsType<UserProfileResponse>(okResult.Value);
            Assert.Equal(userId, profile.UserId);
            Assert.Equal("Nguyen Van A", profile.FullName);
        }
    }
}
