using System.Security.Claims;
using Application.DTOs.Common;
using Application.DTOs.Users;
using Application.Interfaces.Services;
using API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UsersController(_mockUserService.Object);
        }

        private void SetUser(string? userId, string role = "Tenant")
        {
            var claims = new List<Claim>();
            if (userId != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, userId != null ? "TestAuth" : null);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Fact]
        public async Task GetProfile_WhenUserIdMissing_ShouldReturnUnauthorized()
        {
            // Arrange
            SetUser(null);

            // Act
            var result = await _controller.GetProfile();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetProfile_WhenAuthenticated_ShouldReturnOkWithProfile()
        {
            // Arrange
            var userId = "user-123";
            SetUser(userId);
            var profile = new UserProfileResponse
            {
                UserId = userId,
                FullName = "Nguyen Van A",
                PhoneNumber = "0912345678",
                CitizenId = "123456789012"
            };
            _mockUserService.Setup(s => s.GetCurrentUserProfileAsync(userId)).ReturnsAsync(profile);

            // Act
            var result = await _controller.GetProfile();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsType<UserProfileResponse>(okResult.Value);
            Assert.Equal(userId, actual.UserId);
        }

        [Fact]
        public async Task UpdateProfile_WhenUserIdMissing_ShouldReturnUnauthorized()
        {
            // Arrange
            SetUser(null);
            var request = new UpdateProfileRequest { FullName = "New Name" };

            // Act
            var result = await _controller.UpdateProfile(request);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateProfile_WhenAuthenticated_ShouldReturnOkWithMessage()
        {
            // Arrange
            var userId = "user-123";
            SetUser(userId);
            var request = new UpdateProfileRequest { FullName = "New Name", Address = "Da Nang" };
            _mockUserService.Setup(s => s.UpdateCurrentUserProfileAsync(userId, request)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateProfile(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockUserService.Verify(s => s.UpdateCurrentUserProfileAsync(userId, request), Times.Once);
        }

        [Fact]
        public async Task ChangePassword_WhenAuthenticated_ShouldReturnOkWithMessage()
        {
            // Arrange
            var userId = "user-123";
            SetUser(userId);
            var request = new ChangePasswordRequest { CurrentPassword = "OldPassword", NewPassword = "NewPassword123" };
            _mockUserService.Setup(s => s.ChangePasswordAsync(userId, request)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockUserService.Verify(s => s.ChangePasswordAsync(userId, request), Times.Once);
        }

        [Fact]
        public async Task GetUsers_AdminRole_ShouldReturnOkWithPagedUsers()
        {
            // Arrange
            SetUser("admin-1", "Admin");
            var filter = new UserFilterRequest { PageNumber = 1, PageSize = 10 };
            var pagedResult = new PagedResult<AdminUserListItemResponse>(new List<AdminUserListItemResponse>(), 0, 1, 10);
            _mockUserService.Setup(s => s.GetUsersAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetUsers(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsType<PagedResult<AdminUserListItemResponse>>(okResult.Value);
            Assert.Equal(0, actual.TotalCount);
        }

        [Fact]
        public async Task LockUserPermanently_WhenAdminAuthenticated_ShouldReturnOkWithMessage()
        {
            // Arrange
            var adminId = "admin-1";
            var targetUserId = "user-bad";
            SetUser(adminId, "Admin");
            var request = new LockUserRequest { Reason = "Vi phạm nội quy" };
            _mockUserService.Setup(s => s.LockUserPermanentlyAsync(targetUserId, adminId, request.Reason)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.LockUserPermanently(targetUserId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockUserService.Verify(s => s.LockUserPermanentlyAsync(targetUserId, adminId, request.Reason), Times.Once);
        }

        [Fact]
        public async Task UnlockUser_WhenAdminAuthenticated_ShouldReturnOkWithMessage()
        {
            // Arrange
            var adminId = "admin-1";
            var targetUserId = "user-locked";
            SetUser(adminId, "Admin");
            _mockUserService.Setup(s => s.UnlockUserAsync(targetUserId, adminId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UnlockUser(targetUserId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockUserService.Verify(s => s.UnlockUserAsync(targetUserId, adminId), Times.Once);
        }

        [Fact]
        public async Task ResetUserPassword_WhenAdminAuthenticated_ShouldReturnOkWithMessage()
        {
            // Arrange
            var adminId = "admin-1";
            var targetUserId = "user-123";
            SetUser(adminId, "Admin");
            var request = new AdminResetPasswordRequest { NewPassword = "NewRandomPassword123" };
            _mockUserService.Setup(s => s.ResetUserPasswordAsync(targetUserId, request, adminId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ResetUserPassword(targetUserId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockUserService.Verify(s => s.ResetUserPasswordAsync(targetUserId, request, adminId), Times.Once);
        }
    }
}
