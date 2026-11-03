using System.Security.Claims;
using Application.DTOs.Houses;
using Application.Interfaces.Services;
using API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Controllers
{
    public class HousesControllerTests
    {
        private readonly Mock<IHouseService> _mockHouseService;
        private readonly HousesController _controller;

        public HousesControllerTests()
        {
            _mockHouseService = new Mock<IHouseService>();
            _controller = new HousesController(_mockHouseService.Object);
        }

        private void SetUserContext(string userId, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Fact]
        public async Task GetHouses_WhenUserIsAdmin_ShouldCallServiceWithNullUserId()
        {
            // Arrange
            SetUserContext("admin-id", "Admin");
            var expectedList = new List<HouseSummaryResponse>
            {
                new() { HouseId = 1, Name = "Nhà Trọ Xanh", Address = "123 Đường A", TotalRooms = 10, OccupiedRooms = 8 }
            };
            _mockHouseService.Setup(s => s.GetHousesAsync(null)).ReturnsAsync(expectedList);

            // Act
            var result = await _controller.GetHouses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualList = Assert.IsAssignableFrom<IEnumerable<HouseSummaryResponse>>(okResult.Value);
            Assert.Single(actualList);
            _mockHouseService.Verify(s => s.GetHousesAsync(null), Times.Once);
        }

        [Fact]
        public async Task GetHouses_WhenUserIsLandlord_ShouldCallServiceWithLandlordUserId()
        {
            // Arrange
            var landlordId = "landlord-123";
            SetUserContext(landlordId, "Landlord");
            _mockHouseService.Setup(s => s.GetHousesAsync(landlordId)).ReturnsAsync(new List<HouseSummaryResponse>());

            // Act
            var result = await _controller.GetHouses();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockHouseService.Verify(s => s.GetHousesAsync(landlordId), Times.Once);
        }

        [Fact]
        public async Task GetHouseById_ShouldReturnOkWithHouseDetail()
        {
            // Arrange
            var houseId = 1;
            var expectedDto = new HouseDetailResponse
            {
                HouseId = houseId,
                Name = "Nhà Trọ 1",
                Address = "456 Đường B",
                ElectricityUnitPrice = 3500,
                WaterUnitPrice = 20000
            };
            _mockHouseService.Setup(s => s.GetHouseByIdAsync(houseId)).ReturnsAsync(expectedDto);

            // Act
            var result = await _controller.GetHouseById(houseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<HouseDetailResponse>(okResult.Value);
            Assert.Equal(houseId, dto.HouseId);
            Assert.Equal(3500, dto.ElectricityUnitPrice);
        }

        [Fact]
        public async Task CreateHouse_ShouldReturn201CreatedWithHouseId()
        {
            // Arrange
            var userId = "landlord-1";
            SetUserContext(userId, "Landlord");
            var request = new CreateHouseRequest
            {
                Name = "Dãy Trọ Mới",
                Address = "789 Đường C",
                DefaultElectricityPrice = 3500,
                DefaultWaterPrice = 20000
            };
            _mockHouseService.Setup(s => s.CreateHouseAsync(request, userId)).ReturnsAsync(10);

            // Act
            var result = await _controller.CreateHouse(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            _mockHouseService.Verify(s => s.CreateHouseAsync(request, userId), Times.Once);
        }

        [Fact]
        public async Task UpdateHouse_ShouldReturnOkWithMessage()
        {
            // Arrange
            var houseId = 1;
            var request = new UpdateHouseRequest
            {
                Name = "Tên Mới",
                Address = "Địa chỉ mới",
                DefaultElectricityPrice = 4000,
                DefaultWaterPrice = 22000
            };
            _mockHouseService.Setup(s => s.UpdateHouseAsync(houseId, request)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateHouse(houseId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockHouseService.Verify(s => s.UpdateHouseAsync(houseId, request), Times.Once);
        }

        [Fact]
        public async Task DeleteHouse_ShouldReturnNoContent()
        {
            // Arrange
            var houseId = 1;
            _mockHouseService.Setup(s => s.DeleteHouseAsync(houseId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteHouse(houseId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockHouseService.Verify(s => s.DeleteHouseAsync(houseId), Times.Once);
        }
    }
}
