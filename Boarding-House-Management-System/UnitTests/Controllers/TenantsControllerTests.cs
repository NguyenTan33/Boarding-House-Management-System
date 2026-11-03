using Application.DTOs.Tenants;
using Application.Interfaces.Services;
using API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Controllers
{
    public class TenantsControllerTests
    {
        private readonly Mock<ITenantService> _mockTenantService;
        private readonly TenantsController _controller;

        public TenantsControllerTests()
        {
            _mockTenantService = new Mock<ITenantService>();
            _controller = new TenantsController(_mockTenantService.Object);
        }

        [Fact]
        public async Task SearchTenants_ShouldReturnOkWithMatchingTenants()
        {
            // Arrange
            var keyword = "Nguyen";
            int? roomId = 1;
            var expectedList = new List<TenantResponse>
            {
                new() { TenantId = "t-1", FullName = "Nguyen Van A", PhoneNumber = "0912345678", CitizenId = "123456789012" }
            };
            _mockTenantService.Setup(s => s.SearchTenantsAsync(keyword, roomId)).ReturnsAsync(expectedList);

            // Act
            var result = await _controller.SearchTenants(keyword, roomId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tenants = Assert.IsAssignableFrom<IEnumerable<TenantResponse>>(okResult.Value);
            Assert.Single(tenants);
        }

        [Fact]
        public async Task CreateTenant_ShouldReturn201CreatedWithTenantId()
        {
            // Arrange
            var request = new CreateTenantRequest
            {
                FullName = "Tran Thi B",
                PhoneNumber = "0987654321",
                CitizenId = "987654321098",
                Hometown = "Can Tho"
            };
            _mockTenantService.Setup(s => s.CreateTenantAsync(request)).ReturnsAsync("tenant-id-123");

            // Act
            var result = await _controller.CreateTenant(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            _mockTenantService.Verify(s => s.CreateTenantAsync(request), Times.Once);
        }

        [Fact]
        public async Task UpdateTenant_ShouldReturnOkWithMessage()
        {
            // Arrange
            var tenantId = "t-1";
            var request = new UpdateTenantRequest
            {
                FullName = "Nguyen Van A Updated",
                CitizenId = "123456789012",
                Address = "Ha Noi"
            };
            _mockTenantService.Setup(s => s.UpdateTenantAsync(tenantId, request)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateTenant(tenantId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockTenantService.Verify(s => s.UpdateTenantAsync(tenantId, request), Times.Once);
        }
    }
}
