using Application.DTOs.Contracts;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enum;

namespace Application.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly ITenantRepository _tenantRepository;

        public ContractService(
            IContractRepository contractRepository,
            IRoomRepository roomRepository,
            ITenantRepository tenantRepository)
        {
            _contractRepository = contractRepository;
            _roomRepository = roomRepository;
            _tenantRepository = tenantRepository;
        }

        public async Task<int> CreateContractAsync(CreateContractRequest request)
        {
            // 1. Kiểm tra phòng có tồn tại không
            var room = await _roomRepository.GetByIdAsync(request.RoomId);
            if (room == null || !room.IsActive)
                throw new NotFoundException($"Không tìm thấy phòng trọ với ID {request.RoomId}.");

            // 2. Kiểm tra xem phòng có đang có hợp đồng hoạt động không
            var activeContract = await _contractRepository.GetActiveContractByRoomIdAsync(request.RoomId);
            if (activeContract != null)
                throw new ConflictException($"Phòng '{room.RoomNumber}' hiện đang có hợp đồng thuê còn hiệu lực.");

            // 3. Kiểm tra khách thuê có tồn tại không
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);
            if (tenant == null || !tenant.IsActive)
                throw new NotFoundException($"Không tìm thấy tài khoản khách thuê với ID {request.TenantId}.");

            // 4. Sinh mã hợp đồng tự động
            string tenantShortId = request.TenantId.Length >= 4 ? request.TenantId[^4..] : request.TenantId;
            string contractNumber = $"HD-{DateTime.UtcNow:yyyyMMdd}-P{room.RoomNumber}-{tenantShortId}";

            // 5. Khởi tạo thực thể Hợp đồng
            var contract = new Contract(
                contractNumber: contractNumber,
                roomId: request.RoomId,
                accountId: request.TenantId,
                startDate: request.StartDate,
                endDate: request.EndDate,
                rentalPrice: request.RentalPrice,
                depositAmount: request.DepositAmount,
                initialElectricityIndex: request.InitialElectricityIndex,
                initialWaterIndex: request.InitialWaterIndex,
                electricityUnitPrice: request.ElectricityUnitPrice,
                waterUnitPrice: request.WaterUnitPrice,
                internetUnitPrice: request.InternetUnitPrice,
                cleaningUnitPrice: request.CleaningUnitPrice,
                parkingUnitPrice: request.ParkingUnitPrice,
                numberOfRenters: request.NumberOfRenters,
                numberOfVehicles: request.NumberOfVehicles,
                note: request.Note
            );

            // Kích hoạt hợp đồng và đổi trạng thái phòng thành Đang ở (Occupied)
            contract.Activate(request.InitialElectricityIndex, request.InitialWaterIndex);
            room.ChangeStatus(RoomStatus.Occupied);

            await _contractRepository.AddAsync(contract);
            await _roomRepository.UpdateAsync(room);

            return contract.ContractId;
        }

        public async Task<List<ContractResponse>> GetActiveContractsAsync()
        {
            var contracts = await _contractRepository.GetAsync(c => c.Status == ContractStatus.Active);

            return contracts.Select(c => new ContractResponse
            {
                ContractId = c.ContractId,
                ContractNumber = c.ContractNumber,
                RoomId = c.RoomId,
                RoomNumber = c.Room?.RoomNumber ?? string.Empty,
                HouseId = c.Room?.HouseId ?? 0,
                HouseName = c.Room?.House?.HouseName ?? string.Empty,
                TenantId = c.AccountId,
                TenantName = c.Account?.FullName ?? string.Empty,
                TenantPhone = c.Account?.PhoneNumber,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ActualEndDate = c.ActualEndDate,
                RentalPrice = c.RentalPrice,
                DepositAmount = c.DepositAmount,
                DepositRefunded = c.DepositRefunded,
                Status = c.Status
            }).ToList();
        }

        public async Task<List<ContractResponse>> GetExpiringContractsAsync(int days = 30)
        {
            var contracts = await _contractRepository.GetExpiringContractsAsync(days);

            return contracts.Select(c => new ContractResponse
            {
                ContractId = c.ContractId,
                ContractNumber = c.ContractNumber,
                RoomId = c.RoomId,
                RoomNumber = c.Room?.RoomNumber ?? string.Empty,
                HouseId = c.Room?.HouseId ?? 0,
                HouseName = c.Room?.House?.HouseName ?? string.Empty,
                TenantId = c.AccountId,
                TenantName = c.Account?.FullName ?? string.Empty,
                TenantPhone = c.Account?.PhoneNumber,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ActualEndDate = c.ActualEndDate,
                RentalPrice = c.RentalPrice,
                DepositAmount = c.DepositAmount,
                DepositRefunded = c.DepositRefunded,
                Status = c.Status
            }).ToList();
        }

        public async Task TerminateContractAsync(int id, TerminateContractRequest request)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
                throw new NotFoundException($"Không tìm thấy hợp đồng với ID {id}.");

            if (contract.Status != ContractStatus.Active && contract.Status != ContractStatus.PendingRenewal)
                throw new BadRequestException("Chỉ hợp đồng đang hoạt động mới có thể thanh lý.");

            // 1. Thực hiện thanh lý hợp đồng
            decimal actualReturnDeposit = Math.Max(0, request.ReturnDepositAmount - request.Deductions);
            contract.Terminate(
                request.ActualEndDate ?? DateTime.UtcNow, 
                actualReturnDeposit, 
                $"Khấu trừ: {request.Deductions:N0}đ. Lý do: {request.Reason}");

            // 2. Đổi trạng thái phòng thành Trống (Available)
            var room = await _roomRepository.GetByIdAsync(contract.RoomId);
            if (room != null)
            {
                room.ChangeStatus(RoomStatus.Available);
                await _roomRepository.UpdateAsync(room);
            }

            await _contractRepository.UpdateAsync(contract);
        }
    }
}
