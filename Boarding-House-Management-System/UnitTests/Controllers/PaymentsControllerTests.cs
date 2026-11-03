using System.Security.Claims;
using Application.DTOs.Payments;
using Application.Interfaces.Services;
using API.Controllers;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Controllers
{
    public class PaymentsControllerTests
    {
        private readonly Mock<IPaymentService> _mockPaymentService;
        private readonly PaymentsController _controller;

        public PaymentsControllerTests()
        {
            _mockPaymentService = new Mock<IPaymentService>();
            _controller = new PaymentsController(_mockPaymentService.Object);
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
        public async Task GetVietQr_ShouldReturnOkWithQrResponse()
        {
            // Arrange
            var invoiceId = 1;
            var response = new VietQrResponse
            {
                InvoiceId = invoiceId,
                Amount = 3000000,
                BankCode = "MB",
                BankAccountNo = "0987654321",
                AccountName = "NGUYEN VAN CHU TRO",
                QrImageUrl = "https://img.vietqr.io/...",
                QrQuickLink = "https://api.vietqr.io/..."
            };
            _mockPaymentService.Setup(s => s.GenerateVietQrAsync(invoiceId)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetVietQr(invoiceId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsType<VietQrResponse>(okResult.Value);
            Assert.Equal(invoiceId, actual.InvoiceId);
            Assert.Equal("MB", actual.BankCode);
        }

        [Fact]
        public async Task ConfirmCashPayment_ShouldReturnOkWithPaymentResponse()
        {
            // Arrange
            var landlordId = "landlord-1";
            SetUser(landlordId);
            var request = new CashPaymentRequest
            {
                InvoiceId = 1,
                AmountPaid = 3000000,
                Note = "Thu tiền mặt trực tiếp"
            };
            var response = new PaymentResponse
            {
                PaymentId = 1,
                InvoiceId = 1,
                Amount = 3000000,
                Method = PaymentMethod.Cash,
                Status = PaymentStatus.Success
            };
            _mockPaymentService.Setup(s => s.ConfirmCashPaymentAsync(request, landlordId)).ReturnsAsync(response);

            // Act
            var result = await _controller.ConfirmCashPayment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsType<PaymentResponse>(okResult.Value);
            Assert.Equal(PaymentMethod.Cash, actual.Method);
            _mockPaymentService.Verify(s => s.ConfirmCashPaymentAsync(request, landlordId), Times.Once);
        }

        [Fact]
        public async Task SepayWebhook_ShouldReturnOkWithWebhookResult()
        {
            // Arrange
            var request = new SepayWebhookRequest
            {
                Id = 12345,
                Gateway = "MBBank",
                TransactionDate = DateTime.UtcNow,
                AccountNumber = "0987654321",
                Content = "THANHTOAN TRO INV-1",
                TransferType = "in",
                TransferAmount = 3000000
            };
            var webhookResult = new SepayWebhookResultResponse
            {
                Success = true,
                Message = "Xử lý webhook thanh toán thành công",
                InvoiceId = 1
            };
            _mockPaymentService.Setup(s => s.ProcessSepayWebhookAsync(request)).ReturnsAsync(webhookResult);

            // Act
            var result = await _controller.SepayWebhook(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actual = Assert.IsType<SepayWebhookResultResponse>(okResult.Value);
            Assert.True(actual.Success);
            _mockPaymentService.Verify(s => s.ProcessSepayWebhookAsync(request), Times.Once);
        }
    }
}
