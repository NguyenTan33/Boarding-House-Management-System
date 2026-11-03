namespace Domain.Enum
{
    public enum PaymentType
    {
        Invoice = 1,         // Thanh toán hóa đơn tiền phòng/dịch vụ hàng tháng
        ContractDeposit = 2, // Thanh toán tiền đặt cọc giữ phòng
        Other = 3            // Khoản thu khác
    }
}
