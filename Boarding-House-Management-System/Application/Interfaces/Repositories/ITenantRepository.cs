using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ITenantRepository : IGenericRepository<Account>
    {
        Task<List<Account>> SearchTenantsAsync(string keyword);
    }
}
