using Application.DTOs.Contracts;
using Application.Interfaces.Services;
using API.Controllers;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Controllers
{
    public class ContractsControllerTests
    {
        private readonly Mock<IContractService> _mockContractService;
        private readonly ContractsController _controller;

        public ContractsControllerTests()
        {
            _mockContractService = new Mock<IContractService>();
            _controller = new ContractsController(_mockContractService.Object);
        }

        [Fact]
        public async Task CreateContract_ShouldReturn201CreatedWithContractId()
        {
            // Arrange
            var request = new CreateContractRequest
            {
                RoomId = 1,
                TenantId = "tenant-1",
                RentalPrice = 2500000,
                DepositAmount = 2500000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(12),
                InitialElectricityIndex = 100,
                InitialWaterIndex = 50
            };
            _mockContractService.Setup(s => s.CreateContractAsync(request)).ReturnsAsync(15);

            // Act
            var result = await _controller.CreateContract(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            _mockContractService.Verify(s => s.CreateContractAsync(request), Times.Once);
        }

        [Fact]
        public async Task GetActiveContracts_ShouldReturnOkWithActiveContracts()
        {
            // Arrange
            var expectedList = new List<ContractResponse>
            {
                new()
                {
                    ContractId = 1,
                    ContractNumber = "HD-101",
                    RoomNumber = "101",
                    TenantName = "Nguyen Van A",
                    RentalPrice = 2500000,
                    Status = ContractStatus.Active
                }
            };
            _mockContractService.Setup(s => s.GetActiveContractsAsync()).ReturnsAsync(expectedList);

            // Act
            var result = await _controller.GetActiveContracts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var contracts = Assert.IsAssignableFrom<IEnumerable<ContractResponse>>(okResult.Value);
            Assert.Single(contracts);
        }

        [Fact]
        public async Task GetExpiringContracts_ShouldReturnOkWithExpiringContracts()
        {
            // Arrange
            var days = 15;
            var expectedList = new List<ContractResponse>
            {
                new() { ContractId = 2, ContractNumber = "HD-102", RoomNumber = "102", Status = ContractStatus.Active }
            };
            _mockContractService.Setup(s => s.GetExpiringContractsAsync(days)).ReturnsAsync(expectedList);

            // Act
            var result = await _controller.GetExpiringContracts(days);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var contracts = Assert.IsAssignableFrom<IEnumerable<ContractResponse>>(okResult.Value);
            Assert.Single(contracts);
            _mockContractService.Verify(s => s.GetExpiringContractsAsync(days), Times.Once);
        }

        [Fact]
        public async Task TerminateContract_ShouldReturnOkWithMessage()
        {
            // Arrange
            var contractId = 1;
            var request = new TerminateContractRequest
            {
                ReturnDepositAmount = 2500000,
                Deductions = 0,
                Reason = "Hết hạn hợp đồng"
            };
            _mockContractService.Setup(s => s.TerminateContractAsync(contractId, request)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.TerminateContract(contractId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockContractService.Verify(s => s.TerminateContractAsync(contractId, request), Times.Once);
        }
    }
}
