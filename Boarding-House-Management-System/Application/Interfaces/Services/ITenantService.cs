using Application.DTOs.Tenants;

namespace Application.Interfaces.Services
{
    public interface ITenantService
    {
        Task<List<TenantResponse>> SearchTenantsAsync(string keyword, int? roomId = null);
        Task<string> CreateTenantAsync(CreateTenantRequest request);
        Task UpdateTenantAsync(string id, UpdateTenantRequest request);
    }
}
