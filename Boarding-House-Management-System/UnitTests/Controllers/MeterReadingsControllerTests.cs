using System.Security.Claims;
using Application.DTOs.MeterReadings;
using Application.Interfaces.Services;
using API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Controllers
{
    public class MeterReadingsControllerTests
    {
        private readonly Mock<IMeterReadingService> _mockService;
        private readonly MeterReadingsController _controller;

        public MeterReadingsControllerTests()
        {
            _mockService = new Mock<IMeterReadingService>();
            _controller = new MeterReadingsController(_mockService.Object);
        }

        private void SetUser(string userId)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Fact]
        public async Task GetReadingsByHouseAndMonth_ShouldReturnOkWithReadings()
        {
            // Arrange
            var houseId = 1;
            var month = 11;
            var year = 2026;
            var expectedList = new List<MeterReadingResponse>
            {
                new()
                {
                    MeterReadingId = 1,
                    RoomId = 10,
                    RoomNumber = "101",
                    NewElectricityIndex = 150,
                    NewWaterIndex = 30
                }
            };
            _mockService.Setup(s => s.GetReadingsByHouseAndMonthAsync(houseId, month, year)).ReturnsAsync(expectedList);

            // Act
            var result = await _controller.GetReadingsByHouseAndMonth(houseId, month, year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsAssignableFrom<IEnumerable<MeterReadingResponse>>(okResult.Value);
            Assert.Single(actual);
        }

        [Fact]
        public async Task RecordMeterReading_ShouldReturn201Created()
        {
            // Arrange
            var userId = "landlord-1";
            SetUser(userId);
            var request = new RecordMeterReadingRequest
            {
                RoomId = 1,
                Month = 11,
                Year = 2026,
                OldElectricity = 150,
                NewElectricity = 200,
                OldWater = 30,
                NewWater = 40
            };
            var response = new MeterReadingResponse
            {
                MeterReadingId = 1,
                ElectricityUsage = 50,
                WaterUsage = 10,
                IsAnomaly = false
            };
            _mockService.Setup(s => s.RecordMeterReadingAsync(request, userId)).ReturnsAsync(response);

            // Act
            var result = await _controller.RecordMeterReading(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            _mockService.Verify(s => s.RecordMeterReadingAsync(request, userId), Times.Once);
        }

        [Fact]
        public async Task BulkRecordMeterReadings_ShouldReturnOkWithBulkResponse()
        {
            // Arrange
            var userId = "landlord-1";
            SetUser(userId);
            var request = new BulkMeterReadingRequest
            {
                HouseId = 1,
                Month = 11,
                Year = 2026,
                Readings = new List<BulkMeterReadingItem>
                {
                    new() { RoomId = 1, OldElectricity = 150, NewElectricity = 200, OldWater = 20, NewWater = 30 }
                }
            };
            var response = new BulkRecordResponse
            {
                SuccessCount = 1,
                FailureCount = 0
            };
            _mockService.Setup(s => s.BulkRecordMeterReadingsAsync(request, userId)).ReturnsAsync(response);

            // Act
            var result = await _controller.BulkRecordMeterReadings(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsType<BulkRecordResponse>(okResult.Value);
            Assert.Equal(1, actual.SuccessCount);
        }

        [Fact]
        public async Task GetPreviousReading_ShouldReturnOkWithPreviousReadingResponse()
        {
            // Arrange
            var roomId = 1;
            var dto = new PreviousReadingResponse
            {
                RoomId = roomId,
                LastElectricityIndex = 100,
                LastWaterIndex = 20
            };
            _mockService.Setup(s => s.GetPreviousReadingAsync(roomId, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(dto);

            // Act
            var result = await _controller.GetPreviousReading(roomId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsType<PreviousReadingResponse>(okResult.Value);
            Assert.Equal(100, actual.LastElectricityIndex);
        }
    }
}
