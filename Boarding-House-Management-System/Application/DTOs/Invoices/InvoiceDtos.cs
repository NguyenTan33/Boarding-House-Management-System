using Domain.Enum;

namespace Application.DTOs.Invoices
{
    public class GenerateMonthlyInvoicesRequest
    {
        public int HouseId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime DueDate { get; set; }
        public string? StrategyType { get; set; } = "Standard"; // Standard / Tiered
    }

    public class GenerateInvoicesResultResponse
    {
        public int HouseId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalInvoicesCreated { get; set; }
        public decimal TotalAmountBilled { get; set; }
        public List<string> SkippedRoomsInfo { get; set; } = new();
    }

    public class InvoiceFilterRequest
    {
        public int? HouseId { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public InvoiceStatus? Status { get; set; }
        public int? RoomId { get; set; }
        public string? TenantId { get; set; }
    }

    public class InvoiceResponse
    {
        public int InvoiceId { get; set; }
        public string InvoiceCode { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal RoomPrice { get; set; }
        public decimal ElectricityCost { get; set; }
        public decimal WaterCost { get; set; }
        public decimal ServiceCost { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public string StatusName => Status.ToString();
    }

    public class PaymentHistoryDto
    {
        public int PaymentId { get; set; }
        public string PaymentCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public string? Note { get; set; }
    }

    public class InvoiceDetailResponse : InvoiceResponse
    {
        public int ContractId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string HouseName { get; set; } = string.Empty;
        public string HouseAddress { get; set; } = string.Empty;
        public decimal ElectricityUsage { get; set; }
        public decimal ElectricityUnitPrice { get; set; }
        public decimal WaterUsage { get; set; }
        public decimal WaterUnitPrice { get; set; }
        public decimal InternetCost { get; set; }
        public decimal CleaningCost { get; set; }
        public decimal ParkingCost { get; set; }
        public decimal OtherFees { get; set; }
        public string? OtherFeesDescription { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? DiscountReason { get; set; }
        public List<PaymentHistoryDto> Payments { get; set; } = new();
    }
}
