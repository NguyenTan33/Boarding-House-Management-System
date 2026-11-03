using Application.DTOs.Contracts;

namespace Application.Interfaces.Services
{
    public interface IContractService
    {
        Task<int> CreateContractAsync(CreateContractRequest request);
        Task<List<ContractResponse>> GetActiveContractsAsync();
        Task<List<ContractResponse>> GetExpiringContractsAsync(int days = 30);
        Task TerminateContractAsync(int id, TerminateContractRequest request);
    }
}
