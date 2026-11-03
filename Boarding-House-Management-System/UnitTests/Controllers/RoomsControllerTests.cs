using Application.DTOs.Common;
using Application.DTOs.Rooms;
using Application.Interfaces.Services;
using API.Controllers;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Controllers
{
    public class RoomsControllerTests
    {
        private readonly Mock<IRoomService> _mockRoomService;
        private readonly RoomsController _controller;

        public RoomsControllerTests()
        {
            _mockRoomService = new Mock<IRoomService>();
            _controller = new RoomsController(_mockRoomService.Object);
        }

        [Fact]
        public async Task GetRooms_WithFilter_ShouldReturnOkWithPagedRooms()
        {
            // Arrange
            var filter = new RoomFilterRequest { HouseId = 1, PageNumber = 1, PageSize = 10 };
            var pagedResult = new PagedResult<RoomResponse>(
                new List<RoomResponse>
                {
                    new() { RoomId = 1, RoomNumber = "101", Status = RoomStatus.Available, Price = 2500000, MaxRenters = 2 }
                }, 1, 1, 10);

            _mockRoomService.Setup(s => s.GetRoomsAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetRooms(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsType<PagedResult<RoomResponse>>(okResult.Value);
            Assert.Equal(1, actual.TotalCount);
            _mockRoomService.Verify(s => s.GetRoomsAsync(filter), Times.Once);
        }

        [Fact]
        public async Task GetRoomById_ShouldReturnOkWithRoomDetail()
        {
            // Arrange
            var roomId = 1;
            var roomDetail = new RoomDetailResponse
            {
                RoomId = roomId,
                RoomNumber = "101",
                Price = 2500000,
                Status = RoomStatus.Available
            };
            _mockRoomService.Setup(s => s.GetRoomByIdAsync(roomId)).ReturnsAsync(roomDetail);

            // Act
            var result = await _controller.GetRoomById(roomId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsType<RoomDetailResponse>(okResult.Value);
            Assert.Equal(roomId, actual.RoomId);
        }

        [Fact]
        public async Task CreateRoom_ShouldReturn201CreatedWithRoomId()
        {
            // Arrange
            var request = new CreateRoomRequest
            {
                HouseId = 1,
                RoomNumber = "102",
                Price = 3000000,
                Floor = 1,
                Size = 25.5,
                MaxRenters = 3
            };
            _mockRoomService.Setup(s => s.CreateRoomAsync(request)).ReturnsAsync(5);

            // Act
            var result = await _controller.CreateRoom(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            _mockRoomService.Verify(s => s.CreateRoomAsync(request), Times.Once);
        }

        [Fact]
        public async Task UpdateRoom_ShouldReturnNoContent()
        {
            // Arrange
            var roomId = 1;
            var request = new UpdateRoomRequest
            {
                RoomNumber = "101-A",
                Price = 2800000,
                Floor = 1,
                Size = 22.0,
                MaxRenters = 2
            };
            _mockRoomService.Setup(s => s.UpdateRoomAsync(roomId, request)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateRoom(roomId, request);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockRoomService.Verify(s => s.UpdateRoomAsync(roomId, request), Times.Once);
        }

        [Fact]
        public async Task UpdateRoomStatus_ShouldReturnOkWithMessage()
        {
            // Arrange
            var roomId = 1;
            var request = new UpdateRoomStatusRequest { Status = RoomStatus.UnderMaintenance };
            _mockRoomService.Setup(s => s.UpdateRoomStatusAsync(roomId, request)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateRoomStatus(roomId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockRoomService.Verify(s => s.UpdateRoomStatusAsync(roomId, request), Times.Once);
        }

        [Fact]
        public async Task DeleteRoom_ShouldReturnNoContent()
        {
            // Arrange
            var roomId = 1;
            _mockRoomService.Setup(s => s.DeleteRoomAsync(roomId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteRoom(roomId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockRoomService.Verify(s => s.DeleteRoomAsync(roomId), Times.Once);
        }
    }
}
