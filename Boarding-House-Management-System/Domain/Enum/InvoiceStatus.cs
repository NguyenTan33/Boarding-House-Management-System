namespace Domain.Enum
{
    public enum InvoiceStatus
    {
        Unpaid = 1,        // Chưa thanh toán (mới tạo hóa đơn)
        PartiallyPaid = 2, // Đã thanh toán một phần
        Paid = 3,          // Đã thanh toán đầy đủ
        Overdue = 4,       // Quá hạn thanh toán
        Cancelled = 5      // Đã hủy hóa đơn
    }
}
