using Application.DTOs.Houses;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enum;

namespace Application.Services
{
    public class HouseService : IHouseService
    {
        private readonly IHouseRepository _houseRepository;

        public HouseService(IHouseRepository houseRepository)
        {
            _houseRepository = houseRepository;
        }

        public async Task<List<HouseSummaryResponse>> GetHousesAsync(string? ownerId = null)
        {
            var houses = await _houseRepository.GetHousesWithRoomsAsync(ownerId);

            return houses.Select(h => new HouseSummaryResponse
            {
                HouseId = h.HouseId,
                Name = h.HouseName,
                Address = h.Address,
                TotalRooms = h.Rooms.Count(r => r.IsActive),
                OccupiedRooms = h.Rooms.Count(r => r.IsActive && r.Status == RoomStatus.Occupied),
                AvailableRooms = h.Rooms.Count(r => r.IsActive && r.Status == RoomStatus.Available),
                DefaultElectricityPrice = h.ElectricityUnitPrice,
                DefaultWaterPrice = h.WaterUnitPrice
            }).ToList();
        }

        public async Task<HouseDetailResponse> GetHouseByIdAsync(int id)
        {
            var house = await _houseRepository.GetHouseWithRoomsByIdAsync(id);
            if (house == null)
            {
                throw new NotFoundException($"Không tìm thấy dãy trọ với ID {id}.");
            }

            return new HouseDetailResponse
            {
                HouseId = house.HouseId,
                OwnerId = house.OwnerId,
                Name = house.HouseName,
                Address = house.Address,
                Description = house.Description,
                ElectricityUnitPrice = house.ElectricityUnitPrice,
                WaterUnitPrice = house.WaterUnitPrice,
                InternetUnitPrice = house.InternetUnitPrice,
                CleaningUnitPrice = house.CleaningUnitPrice,
                ParkingUnitPrice = house.ParkingUnitPrice,
                TotalRooms = house.Rooms.Count(r => r.IsActive)
            };
        }

        public async Task<int> CreateHouseAsync(CreateHouseRequest request, string? ownerId = null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("Tên dãy trọ không được để trống.");

            if (string.IsNullOrWhiteSpace(request.Address))
                throw new BadRequestException("Địa chỉ dãy trọ không được để trống.");

            var house = new House(
                houseName: request.Name,
                address: request.Address,
                electricityUnitPrice: request.DefaultElectricityPrice,
                waterUnitPrice: request.DefaultWaterPrice,
                internetUnitPrice: request.DefaultInternetPrice,
                cleaningUnitPrice: request.DefaultCleaningPrice,
                parkingUnitPrice: request.DefaultParkingPrice,
                description: request.Description,
                ownerId: ownerId
            );

            await _houseRepository.AddAsync(house);
            return house.HouseId;
        }

        public async Task UpdateHouseAsync(int id, UpdateHouseRequest request)
        {
            var house = await _houseRepository.GetByIdAsync(id);
            if (house == null || !house.IsActive)
                throw new NotFoundException($"Không tìm thấy dãy trọ với ID {id}.");

            house.UpdateInfo(request.Name, request.Address, request.Description);
            house.UpdateServicePrices(
                request.DefaultElectricityPrice,
                request.DefaultWaterPrice,
                request.DefaultInternetPrice,
                request.DefaultCleaningPrice,
                request.DefaultParkingPrice);

            await _houseRepository.UpdateAsync(house);
        }

        public async Task DeleteHouseAsync(int id)
        {
            var house = await _houseRepository.GetHouseWithRoomsByIdAsync(id);
            if (house == null || !house.IsActive)
                throw new NotFoundException($"Không tìm thấy dãy trọ với ID {id}.");

            // Quy tắc nghiệp vụ: Chỉ cho phép xóa nếu dãy này không có phòng nào
            if (house.Rooms.Any(r => r.IsActive))
            {
                throw new BadRequestException("Chỉ cho phép xóa dãy trọ khi không còn phòng trọ nào đang hoạt động.");
            }

            house.Deactivate();
            await _houseRepository.UpdateAsync(house);
        }
    }
}
