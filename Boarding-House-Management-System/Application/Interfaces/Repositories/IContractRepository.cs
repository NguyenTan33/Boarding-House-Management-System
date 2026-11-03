using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IContractRepository : IGenericRepository<Contract>
    {
        Task<Contract?> GetActiveContractByRoomIdAsync(int roomId);
        Task<List<Contract>> GetExpiringContractsAsync(int daysThreshold);
    }
}
