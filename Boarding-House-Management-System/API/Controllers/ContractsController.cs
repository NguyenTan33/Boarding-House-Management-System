using Application.DTOs.Contracts;
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
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _contractService;

        public ContractsController(IContractService contractService)
        {
            _contractService = contractService;
        }

        // 1. POST /api/contracts: Lập hợp đồng gán khách vào phòng
        [HttpPost]
        public async Task<IActionResult> CreateContract([FromBody] CreateContractRequest request)
        {
            int contractId = await _contractService.CreateContractAsync(request);
            return StatusCode(StatusCodes.Status201Created, new { contractId, message = "Lập hợp đồng và kích hoạt nhận phòng thành công." });
        }

        // 2. GET /api/contracts/active: Danh sách hợp đồng đang còn hiệu lực
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveContracts()
        {
            var contracts = await _contractService.GetActiveContractsAsync();
            return Ok(contracts);
        }

        // 3. GET /api/contracts/expiring: Danh sách các hợp đồng sắp hết hạn trong X ngày
        [HttpGet("expiring")]
        public async Task<IActionResult> GetExpiringContracts([FromQuery] int days = 30)
        {
            var contracts = await _contractService.GetExpiringContractsAsync(days);
            return Ok(contracts);
        }

        // 4. POST /api/contracts/{id}/terminate: Thanh lý hợp đồng, trả phòng, hoàn cọc
        [HttpPost("{id:int}/terminate")]
        public async Task<IActionResult> TerminateContract(int id, [FromBody] TerminateContractRequest request)
        {
            await _contractService.TerminateContractAsync(id, request);
            return Ok(new { message = "Thanh lý hợp đồng và trả phòng thành công." });
        }
    }
}
