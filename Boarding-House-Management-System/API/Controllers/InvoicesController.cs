using System.Security.Claims;
using Application.DTOs.Invoices;
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
    public class InvoicesController : ControllerBase
    {
        private readonly IBillingService _billingService;

        public InvoicesController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        // 1. POST /api/invoices/generate-monthly: Quét toàn bộ phòng đang thuê -> Tính toán điện nước -> Sinh hóa đơn hàng loạt
        [HttpPost("generate-monthly")]
        [Authorize(Roles = "Landlord,Admin")]
        [EnableRateLimiting("HeavyPolicy")]
        public async Task<IActionResult> GenerateMonthlyInvoices([FromBody] GenerateMonthlyInvoicesRequest request)
        {
            var result = await _billingService.GenerateMonthlyInvoicesAsync(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        // 2. GET /api/invoices: Lọc danh sách hóa đơn theo tháng, năm, trạng thái, phòng
        [HttpGet]
        public async Task<IActionResult> GetInvoices([FromQuery] InvoiceFilterRequest filter)
        {
            // Bảo mật: Nếu người dùng là Tenant, chỉ cho phép tra cứu hóa đơn của chính mình
            bool isTenant = User.IsInRole("Tenant");
            if (isTenant)
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                filter.TenantId = userId;
            }

            var invoices = await _billingService.GetInvoicesAsync(filter);
            return Ok(invoices);
        }

        // 3. GET /api/invoices/{id}: Chi tiết 1 hóa đơn đầy đủ các khoản thu và lịch sử thanh toán
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            var invoice = await _billingService.GetInvoiceByIdAsync(id);

            // Nếu là Tenant thì chỉ được xem hóa đơn đứng tên mình
            bool isTenant = User.IsInRole("Tenant");
            if (isTenant)
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                // Kiểm tra phân quyền truy cập hóa đơn của tenant
                var invoices = await _billingService.GetInvoicesAsync(new InvoiceFilterRequest { TenantId = userId });
                if (!invoices.Any(i => i.InvoiceId == id))
                {
                    return Forbid();
                }
            }

            return Ok(invoice);
        }

        // 4. PATCH /api/invoices/{id}/cancel: Hủy hóa đơn (để chỉnh sửa nếu ghi nhầm số điện nước)
        [HttpPatch("{id:int}/cancel")]
        [Authorize(Roles = "Landlord,Admin")]
        public async Task<IActionResult> CancelInvoice(int id, [FromBody] string? reason)
        {
            await _billingService.CancelInvoiceAsync(id, reason);
            return Ok(new { message = "Hủy hóa đơn thành công và đã mở khóa chỉ số điện nước tương ứng." });
        }
    }
}
