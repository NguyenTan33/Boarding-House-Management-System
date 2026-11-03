using Application.DTOs.Tenants;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services
{
    public class TenantService : ITenantService
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IContractRepository _contractRepository;

        public TenantService(ITenantRepository tenantRepository, IContractRepository contractRepository)
        {
            _tenantRepository = tenantRepository;
            _contractRepository = contractRepository;
        }

        public async Task<List<TenantResponse>> SearchTenantsAsync(string keyword, int? roomId = null)
        {
            List<Account> tenants;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                var all = await _tenantRepository.GetAllAsync();
                tenants = all.Where(t => t.IsActive).ToList();
            }
            else
            {
                tenants = await _tenantRepository.SearchTenantsAsync(keyword);
            }

            if (roomId.HasValue)
            {
                var contract = await _contractRepository.GetActiveContractByRoomIdAsync(roomId.Value);
                if (contract != null)
                {
                    tenants = tenants.Where(t => t.Id == contract.AccountId).ToList();
                }
            }

            return tenants.Select(t => new TenantResponse
            {
                TenantId = t.Id,
                FullName = t.FullName,
                PhoneNumber = t.PhoneNumber ?? string.Empty,
                CitizenId = t.CitizenId,
                Email = t.Email,
                DateOfBirth = t.DateOfBirth,
                Address = t.Address,
                Gender = t.Gender,
                IsActive = t.IsActive,
                ActiveContractsCount = t.Contracts?.Count(c => c.Status == Domain.Enum.ContractStatus.Active) ?? 0
            }).ToList();
        }

        public async Task<string> CreateTenantAsync(CreateTenantRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new BadRequestException("Họ và tên khách thuê không được để trống.");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                throw new BadRequestException("Số điện thoại không được để trống.");

            if (string.IsNullOrWhiteSpace(request.CitizenId))
                throw new BadRequestException("Số CCCD không được để trống.");

            // Kiểm tra trùng số CCCD
            bool citizenExists = await _tenantRepository.AnyAsync(t => t.CitizenId == request.CitizenId && t.IsActive);
            if (citizenExists)
                throw new ConflictException($"Khách thuê với số CCCD '{request.CitizenId}' đã tồn tại trong hệ thống.");

            // Tạo tài khoản khách thuê
            string userName = !string.IsNullOrWhiteSpace(request.PhoneNumber) ? request.PhoneNumber : request.CitizenId;
            var tenant = new Account(
                userName: userName,
                email: request.Email ?? $"{userName}@boardinghouse.local",
                fullName: request.FullName,
                citizenId: request.CitizenId,
                phoneNumber: request.PhoneNumber,
                dateOfBirth: request.DateOfBirth,
                address: request.Hometown
            );

            await _tenantRepository.AddAsync(tenant);
            return tenant.Id;
        }

        public async Task UpdateTenantAsync(string id, UpdateTenantRequest request)
        {
            var tenant = await _tenantRepository.GetByIdAsync(id);
            if (tenant == null || !tenant.IsActive)
                throw new NotFoundException($"Không tìm thấy thông tin khách thuê với ID {id}.");

            tenant.UpdateProfile(
                fullName: request.FullName,
                citizenId: request.CitizenId,
                phoneNumber: request.PhoneNumber,
                dateOfBirth: request.DateOfBirth,
                address: request.Address,
                gender: request.Gender,
                avatarUrl: null
            );

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                tenant.PhoneNumber = request.PhoneNumber;
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                tenant.Email = request.Email;
            }

            await _tenantRepository.UpdateAsync(tenant);
        }
    }
}
