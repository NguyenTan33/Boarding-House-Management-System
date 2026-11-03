using Domain.Enum;

namespace Application.DTOs.Rooms
{
    public class RoomFilterRequest
    {
        public int? HouseId { get; set; }
        public RoomStatus? Status { get; set; }
        public int? Floor { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class CreateRoomRequest
    {
        public int HouseId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Floor { get; set; }
        public double Size { get; set; }
        public int MaxRenters { get; set; }
        public bool HasBalcony { get; set; } = false;
    }

    public class UpdateRoomRequest
    {
        public string RoomNumber { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Floor { get; set; }
        public double Size { get; set; }
        public int MaxRenters { get; set; }
        public bool HasBalcony { get; set; }
    }

    public class UpdateRoomStatusRequest
    {
        public RoomStatus Status { get; set; }
    }

    public class RoomResponse
    {
        public int RoomId { get; set; }
        public int HouseId { get; set; }
        public string HouseName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Floor { get; set; }
        public double Size { get; set; }
        public int MaxRenters { get; set; }
        public bool HasBalcony { get; set; }
        public RoomStatus Status { get; set; }
        public string StatusName => Status.ToString();
    }

    public class ActiveContractSimpleDto
    {
        public int ContractId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string? TenantPhone { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal RentalPrice { get; set; }
        public decimal DepositAmount { get; set; }
    }

    public class RoomDetailResponse : RoomResponse
    {
        public ActiveContractSimpleDto? ActiveContract { get; set; }
    }
}
