using Application.DTOs.Common;
using Application.DTOs.Rooms;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enum;

namespace Application.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IHouseRepository _houseRepository;

        public RoomService(IRoomRepository roomRepository, IHouseRepository houseRepository)
        {
            _roomRepository = roomRepository;
            _houseRepository = houseRepository;
        }

        public async Task<PagedResult<RoomResponse>> GetRoomsAsync(RoomFilterRequest filter)
        {
            int page = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var rooms = await _roomRepository.GetAsync(r => 
                r.IsActive &&
                (!filter.HouseId.HasValue || r.HouseId == filter.HouseId.Value) &&
                (!filter.Status.HasValue || r.Status == filter.Status.Value) &&
                (!filter.Floor.HasValue || r.Floor == filter.Floor.Value) &&
                (!filter.MinPrice.HasValue || r.Price >= filter.MinPrice.Value) &&
                (!filter.MaxPrice.HasValue || r.Price <= filter.MaxPrice.Value)
            );

            int totalCount = rooms.Count;

            var pagedRooms = rooms
                .OrderBy(r => r.Floor)
                .ThenBy(r => r.RoomNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RoomResponse
                {
                    RoomId = r.RoomId,
                    HouseId = r.HouseId,
                    HouseName = r.House?.HouseName ?? string.Empty,
                    RoomNumber = r.RoomNumber,
                    Price = r.Price,
                    Floor = r.Floor,
                    Size = r.Size,
                    MaxRenters = r.MaxRenters,
                    HasBalcony = r.HasBalcony,
                    Status = r.Status
                })
                .ToList();

            return new PagedResult<RoomResponse>(pagedRooms, totalCount, page, pageSize);
        }

        public async Task<RoomDetailResponse> GetRoomByIdAsync(int id)
        {
            var room = await _roomRepository.GetRoomWithActiveContractAsync(id);
            if (room == null || !room.IsActive)
                throw new NotFoundException($"Không tìm thấy phòng trọ với ID {id}.");

            var activeContract = room.Contracts.FirstOrDefault(c => c.Status == ContractStatus.Active);

            return new RoomDetailResponse
            {
                RoomId = room.RoomId,
                HouseId = room.HouseId,
                HouseName = room.House?.HouseName ?? string.Empty,
                RoomNumber = room.RoomNumber,
                Price = room.Price,
                Floor = room.Floor,
                Size = room.Size,
                MaxRenters = room.MaxRenters,
                HasBalcony = room.HasBalcony,
                Status = room.Status,
                ActiveContract = activeContract != null ? new ActiveContractSimpleDto
                {
                    ContractId = activeContract.ContractId,
                    ContractNumber = activeContract.ContractNumber,
                    TenantId = activeContract.AccountId,
                    TenantName = activeContract.Account?.FullName ?? string.Empty,
                    TenantPhone = activeContract.Account?.PhoneNumber,
                    StartDate = activeContract.StartDate,
                    EndDate = activeContract.EndDate,
                    RentalPrice = activeContract.RentalPrice,
                    DepositAmount = activeContract.DepositAmount
                } : null
            };
        }

        public async Task<int> CreateRoomAsync(CreateRoomRequest request)
        {
            var house = await _houseRepository.GetByIdAsync(request.HouseId);
            if (house == null || !house.IsActive)
                throw new NotFoundException($"Dãy trọ với ID {request.HouseId} không tồn tại.");

            // Kiểm tra trùng số phòng trong cùng 1 dãy trọ
            bool exists = await _roomRepository.IsRoomNumberExistsAsync(request.HouseId, request.RoomNumber);
            if (exists)
                throw new ConflictException($"Phòng số '{request.RoomNumber}' đã tồn tại trong dãy trọ này.");

            var room = new Room(
                houseId: request.HouseId,
                roomNumber: request.RoomNumber,
                price: request.Price,
                floor: request.Floor,
                size: request.Size,
                maxRenters: request.MaxRenters,
                hasBalcony: request.HasBalcony
            );

            await _roomRepository.AddAsync(room);
            return room.RoomId;
        }

        public async Task UpdateRoomAsync(int id, UpdateRoomRequest request)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null || !room.IsActive)
                throw new NotFoundException($"Không tìm thấy phòng trọ với ID {id}.");

            // Nếu đổi số phòng, kiểm tra xem số phòng mới có trùng không
            if (!room.RoomNumber.Equals(request.RoomNumber.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                bool exists = await _roomRepository.IsRoomNumberExistsAsync(room.HouseId, request.RoomNumber, id);
                if (exists)
                    throw new ConflictException($"Phòng số '{request.RoomNumber}' đã tồn tại trong dãy trọ này.");

                room.UpdateRoomNumber(request.RoomNumber);
            }

            room.UpdatePrice(request.Price);
            room.UpdateFloor(request.Floor);
            room.UpdateSize(request.Size);
            room.UpdateMaxRenters(request.MaxRenters);
            room.SetBalcony(request.HasBalcony);

            await _roomRepository.UpdateAsync(room);
        }

        public async Task UpdateRoomStatusAsync(int id, UpdateRoomStatusRequest request)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null || !room.IsActive)
                throw new NotFoundException($"Không tìm thấy phòng trọ với ID {id}.");

            room.ChangeStatus(request.Status);
            await _roomRepository.UpdateAsync(room);
        }

        public async Task DeleteRoomAsync(int id)
        {
            var room = await _roomRepository.GetRoomWithActiveContractAsync(id);
            if (room == null || !room.IsActive)
                throw new NotFoundException($"Không tìm thấy phòng trọ với ID {id}.");

            // Quy tắc nghiệp vụ: Chặn lại nếu phòng đang có người ở
            if (room.Status == RoomStatus.Occupied || room.Contracts.Any(c => c.Status == ContractStatus.Active))
            {
                throw new BadRequestException("Không thể xóa phòng trọ đang có người thuê. Vui lòng thanh lý hợp đồng trước.");
            }

            room.Deactivate();
            await _roomRepository.UpdateAsync(room);
        }
    }
}
