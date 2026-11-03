using Domain.Enum;

namespace Application.DTOs.Contracts
{
    public class CreateContractRequest
    {
        public int RoomId { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal RentalPrice { get; set; }
        public decimal DepositAmount { get; set; }
        public int NumberOfRenters { get; set; } = 1;
        public int NumberOfVehicles { get; set; } = 0;
        public decimal InitialElectricityIndex { get; set; }
        public decimal InitialWaterIndex { get; set; }
        public decimal ElectricityUnitPrice { get; set; }
        public decimal WaterUnitPrice { get; set; }
        public decimal InternetUnitPrice { get; set; }
        public decimal CleaningUnitPrice { get; set; }
        public decimal ParkingUnitPrice { get; set; }
        public string? Note { get; set; }
    }

    public class TerminateContractRequest
    {
        public decimal ReturnDepositAmount { get; set; }
        public decimal Deductions { get; set; } = 0;
        public string? Reason { get; set; }
        public DateTime? ActualEndDate { get; set; }
    }

    public class ContractResponse
    {
        public int ContractId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int HouseId { get; set; }
        public string HouseName { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string? TenantPhone { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal RentalPrice { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal? DepositRefunded { get; set; }
        public ContractStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public int DaysRemaining => Math.Max(0, (EndDate.Date - DateTime.UtcNow.Date).Days);
    }
}
