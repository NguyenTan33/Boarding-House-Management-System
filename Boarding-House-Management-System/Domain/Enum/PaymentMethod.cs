namespace Domain.Enum
{
    public enum PaymentMethod
    {
        Cash = 1,         // Tiền mặt
        BankTransfer = 2, // Chuyển khoản ngân hàng (VietQR)
        PayOS = 3,        // Cổng PayOS (tự động webhook VietQR)
        VNPay = 4,        // Cổng VNPay
        MoMo = 5          // Ví MoMo
    }
}
