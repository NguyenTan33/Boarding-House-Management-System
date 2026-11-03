using System.Security.Claims;
using Application.DTOs.Invoices;
using Application.Interfaces.Services;
using API.Controllers;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Controllers
{
    public class InvoicesControllerTests
    {
        private readonly Mock<IBillingService> _mockBillingService;
        private readonly InvoicesController _controller;

        public InvoicesControllerTests()
        {
            _mockBillingService = new Mock<IBillingService>();
            _controller = new InvoicesController(_mockBillingService.Object);
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
        public async Task GenerateMonthlyInvoices_ShouldReturn201Created()
        {
            // Arrange
            SetUserContext("landlord-1", "Landlord");
            var request = new GenerateMonthlyInvoicesRequest { HouseId = 1, Month = 11, Year = 2026 };
            var response = new GenerateInvoicesResultResponse
            {
                TotalInvoicesCreated = 5,
                TotalAmountBilled = 15000000
            };
            _mockBillingService.Setup(s => s.GenerateMonthlyInvoicesAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.GenerateMonthlyInvoices(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            _mockBillingService.Verify(s => s.GenerateMonthlyInvoicesAsync(request), Times.Once);
        }

        [Fact]
        public async Task GetInvoices_WhenUserIsTenant_ShouldEnforceTenantIdFilter()
        {
            // Arrange
            var tenantId = "tenant-xyz";
            SetUserContext(tenantId, "Tenant");
            var filter = new InvoiceFilterRequest { Month = 11, Year = 2026 };

            _mockBillingService.Setup(s => s.GetInvoicesAsync(It.Is<InvoiceFilterRequest>(f => f.TenantId == tenantId)))
                .ReturnsAsync(new List<InvoiceResponse>());

            // Act
            var result = await _controller.GetInvoices(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(tenantId, filter.TenantId);
        }

        [Fact]
        public async Task GetInvoiceById_WhenUserIsLandlord_ShouldReturnOk()
        {
            // Arrange
            SetUserContext("landlord-1", "Landlord");
            var invoiceId = 10;
            var invoiceDetail = new InvoiceDetailResponse
            {
                InvoiceId = invoiceId,
                InvoiceCode = "INV-001",
                TotalAmount = 3000000,
                Status = InvoiceStatus.Unpaid
            };
            _mockBillingService.Setup(s => s.GetInvoiceByIdAsync(invoiceId)).ReturnsAsync(invoiceDetail);

            // Act
            var result = await _controller.GetInvoiceById(invoiceId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsType<InvoiceDetailResponse>(okResult.Value);
            Assert.Equal(invoiceId, actual.InvoiceId);
        }

        [Fact]
        public async Task GetInvoiceById_WhenUserIsTenantAndDoesNotOwnInvoice_ShouldReturnForbid()
        {
            // Arrange
            var tenantId = "tenant-1";
            SetUserContext(tenantId, "Tenant");
            var invoiceId = 10;
            var otherInvoiceDetail = new InvoiceDetailResponse { InvoiceId = invoiceId };

            _mockBillingService.Setup(s => s.GetInvoiceByIdAsync(invoiceId)).ReturnsAsync(otherInvoiceDetail);
            // Returns an empty list of invoices for this tenant, so invoice 10 is not owned by them
            _mockBillingService.Setup(s => s.GetInvoicesAsync(It.Is<InvoiceFilterRequest>(f => f.TenantId == tenantId)))
                .ReturnsAsync(new List<InvoiceResponse>());

            // Act
            var result = await _controller.GetInvoiceById(invoiceId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task CancelInvoice_ShouldReturnOkWithMessage()
        {
            // Arrange
            SetUserContext("landlord-1", "Landlord");
            var invoiceId = 1;
            var reason = "Ghi nhầm số điện";
            _mockBillingService.Setup(s => s.CancelInvoiceAsync(invoiceId, reason)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CancelInvoice(invoiceId, reason);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockBillingService.Verify(s => s.CancelInvoiceAsync(invoiceId, reason), Times.Once);
        }
    }
}
