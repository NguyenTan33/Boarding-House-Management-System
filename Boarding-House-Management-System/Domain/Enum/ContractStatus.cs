namespace Domain.Enum
{
    public enum ContractStatus
    {
        Draft = 1,       // Bản nháp / Chờ ký
        Active = 2,      // Đang có hiệu lực (khách đang ở)
        PendingRenewal = 3, // Sắp hết hạn / Chờ gia hạn
        Terminated = 4,  // Đã thanh lý / Kết thúc hợp đồng
        Cancelled = 5    // Đã hủy (vd: hủy cọc trước khi nhận phòng)
    }
}
