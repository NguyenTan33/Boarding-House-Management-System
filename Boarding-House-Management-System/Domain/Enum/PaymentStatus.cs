namespace Domain.Enum
{
    public enum PaymentStatus
    {
        Pending = 1,   // Đang chờ thanh toán (mới tạo QR hoặc chờ nộp tiền)
        Success = 2,   // Thanh toán thành công
        Failed = 3,    // Thanh toán thất bại
        Cancelled = 4, // Đã hủy giao dịch
        Refunded = 5   // Đã hoàn tiền
    }
}
