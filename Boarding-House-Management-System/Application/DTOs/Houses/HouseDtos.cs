namespace Application.DTOs.Houses
{
    public class CreateHouseRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal DefaultElectricityPrice { get; set; }
        public decimal DefaultWaterPrice { get; set; }
        public decimal DefaultInternetPrice { get; set; }
        public decimal DefaultCleaningPrice { get; set; }
        public decimal DefaultParkingPrice { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateHouseRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal DefaultElectricityPrice { get; set; }
        public decimal DefaultWaterPrice { get; set; }
        public decimal DefaultInternetPrice { get; set; }
        public decimal DefaultCleaningPrice { get; set; }
        public decimal DefaultParkingPrice { get; set; }
        public string? Description { get; set; }
    }

    public class HouseSummaryResponse
    {
        public int HouseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public decimal DefaultElectricityPrice { get; set; }
        public decimal DefaultWaterPrice { get; set; }
    }

    public class HouseDetailResponse
    {
        public int HouseId { get; set; }
        public string? OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal ElectricityUnitPrice { get; set; }
        public decimal WaterUnitPrice { get; set; }
        public decimal InternetUnitPrice { get; set; }
        public decimal CleaningUnitPrice { get; set; }
        public decimal ParkingUnitPrice { get; set; }
        public int TotalRooms { get; set; }
    }
}
