using Application.DTOs.Tenants;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Landlord,Admin")]
    [EnableRateLimiting("StandardPolicy")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantsController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        // 1. GET /api/tenants: Tìm kiếm khách thuê theo tên, CCCD hoặc số điện thoại
        [HttpGet]
        public async Task<IActionResult> SearchTenants([FromQuery] string? keyword, [FromQuery] int? roomId)
        {
            var tenants = await _tenantService.SearchTenantsAsync(keyword ?? string.Empty, roomId);
            return Ok(tenants);
        }

        // 2. POST /api/tenants: Tạo hồ sơ khách thuê mới
        [HttpPost]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
        {
            string tenantId = await _tenantService.CreateTenantAsync(request);
            return StatusCode(StatusCodes.Status201Created, new { tenantId, message = "Thêm hồ sơ khách thuê thành công." });
        }

        // 3. PUT /api/tenants/{id}: Cập nhật thông tin khách thuê
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTenant(string id, [FromBody] UpdateTenantRequest request)
        {
            await _tenantService.UpdateTenantAsync(id, request);
            return Ok(new { message = "Cập nhật hồ sơ khách thuê thành công." });
        }
    }
}
