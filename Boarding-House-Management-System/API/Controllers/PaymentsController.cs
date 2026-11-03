using System.Security.Claims;
using Application.DTOs.Payments;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("StandardPolicy")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // 1. GET /api/payments/{invoiceId}/vietqr: Lấy mã QR ngân hàng động VietQR (kèm đúng STK, số tiền, nội dung)
        [HttpGet("{invoiceId:int}/vietqr")]
        public async Task<IActionResult> GetVietQr(int invoiceId)
        {
            var qrResponse = await _paymentService.GenerateVietQrAsync(invoiceId);
            return Ok(qrResponse);
        }

        // 2. POST /api/payments/cash: Xác nhận khách đã đóng tiền mặt trực tiếp cho chủ trọ
        [HttpPost("cash")]
        [Authorize(Roles = "Landlord,Admin")]
        public async Task<IActionResult> ConfirmCashPayment([FromBody] CashPaymentRequest request)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var paymentResponse = await _paymentService.ConfirmCashPaymentAsync(request, userId);
            return Ok(paymentResponse);
        }

        // 3. POST /api/payments/sepay-webhook: Cổng nhận thông báo tự động khi khách quét QR thanh toán
        [HttpPost("sepay-webhook")]
        [AllowAnonymous]
        [EnableRateLimiting("WebhookPolicy")]
        public async Task<IActionResult> SepayWebhook([FromBody] SepayWebhookRequest request)
        {
            var result = await _paymentService.ProcessSepayWebhookAsync(request);
            return Ok(result);
        }
    }
}
