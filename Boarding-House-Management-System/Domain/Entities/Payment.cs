using Domain.Enum;
using Domain.Exceptions;

namespace Domain.Entities
{
    public class Payment
    {
        // Khóa chính
        public int PaymentId { get; private set; }

        // Mã thanh toán nội bộ (vd: "PAY-202611-00123", dùng làm nội dung chuyển khoản VietQR)
        public string PaymentCode { get; private set; } = string.Empty;

        // Mã giao dịch từ phía Ngân hàng hoặc Cổng thanh toán (PayOS, VNPay, MoMo...)
        public string? TransactionId { get; private set; }

        // Các liên kết quan hệ
        public int? InvoiceId { get; private set; }                       // Hóa đơn thanh toán (nếu có)
        public Invoice? Invoice { get; private set; }

        public int? ContractId { get; private set; }                      // Hợp đồng liên quan (nếu đóng cọc hoặc thanh toán)
        public Contract? Contract { get; private set; }

        public string AccountId { get; private set; } = string.Empty;     // Khách thuê thực hiện thanh toán
        public Account? Account { get; private set; }

        // Số tiền thanh toán (VNĐ)
        public decimal Amount { get; private set; }

        // Phương thức & Loại giao dịch
        public PaymentMethod Method { get; private set; } = PaymentMethod.BankTransfer; // Tiền mặt / Chuyển khoản QR
        public PaymentType Type { get; private set; } = PaymentType.Invoice;            // Hóa đơn / Đặt cọc
        public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;

        // Thời gian
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow; // Thời điểm tạo lệnh thanh toán
        public DateTime? PaidAt { get; private set; }                      // Thời điểm giao dịch thành công

        // Bằng chứng thanh toán & Đối soát
        public string? ProofImageUrl { get; private set; }                 // Ảnh chụp biên lai chuyển khoản thành công
        public string? ConfirmedByAccountId { get; private set; }          // Người xác nhận (chủ trọ xác nhận nếu thu tiền mặt)
        public string? Note { get; private set; }                          // Ghi chú giao dịch

        // Constructor cho EF Core
        private Payment() { }

        // Constructor nghiệp vụ
        public Payment(
            string paymentCode,
            string accountId,
            decimal amount,
            PaymentMethod method,
            PaymentType type,
            int? invoiceId = null,
            int? contractId = null,
            string? note = null)
        {
            Validate(paymentCode, accountId, amount);

            PaymentCode = paymentCode.Trim();
            AccountId = accountId;
            Amount = amount;
            Method = method;
            Type = type;
            InvoiceId = invoiceId;
            ContractId = contractId;
            Note = note?.Trim();
            CreatedAt = DateTime.UtcNow;
            Status = PaymentStatus.Pending;
        }

        // Xác nhận thanh toán thành công (dành cho Webhook tự động hoặc Chủ trọ bấm xác nhận tiền mặt)
        public void MarkAsSuccess(string? transactionId = null, string? confirmedByAccountId = null, string? note = null)
        {
            if (Status == PaymentStatus.Success)
                throw new ValidationException("Giao dịch này đã được xác nhận thanh toán trước đó.");

            if (Status == PaymentStatus.Cancelled)
                throw new ValidationException("Giao dịch đã bị hủy, không thể xác nhận thành công.");

            Status = PaymentStatus.Success;
            PaidAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(transactionId))
                TransactionId = transactionId.Trim();

            if (!string.IsNullOrWhiteSpace(confirmedByAccountId))
                ConfirmedByAccountId = confirmedByAccountId.Trim();

            if (!string.IsNullOrWhiteSpace(note))
                Note = string.IsNullOrWhiteSpace(Note) ? note.Trim() : $"{Note}\n{note.Trim()}";
        }

        // Đánh dấu thanh toán thất bại
        public void MarkAsFailed(string reason)
        {
            if (Status == PaymentStatus.Success)
                throw new ValidationException("Giao dịch đã thành công, không thể chuyển sang thất bại.");

            Status = PaymentStatus.Failed;
            Note = string.IsNullOrWhiteSpace(Note) ? $"[Thất bại]: {reason.Trim()}" : $"{Note}\n[Thất bại]: {reason.Trim()}";
        }

        // Hủy giao dịch thanh toán
        public void Cancel(string reason)
        {
            if (Status == PaymentStatus.Success)
                throw new ValidationException("Giao dịch đã hoàn tất thanh toán, không thể hủy.");

            Status = PaymentStatus.Cancelled;
            Note = string.IsNullOrWhiteSpace(Note) ? $"[Hủy]: {reason.Trim()}" : $"{Note}\n[Hủy]: {reason.Trim()}";
        }

        // Đính kèm ảnh biên nhận / hóa đơn chuyển khoản ngân hàng
        public void AttachProofImage(string proofImageUrl)
        {
            if (string.IsNullOrWhiteSpace(proofImageUrl))
                throw new ValidationException("Đường dẫn ảnh biên nhận không được để trống.");

            ProofImageUrl = proofImageUrl.Trim();
        }

        // Hoàn tiền giao dịch
        public void Refund(string reason)
        {
            if (Status != PaymentStatus.Success)
                throw new ValidationException("Chỉ giao dịch đã thanh toán thành công mới có thể hoàn tiền.");

            Status = PaymentStatus.Refunded;
            Note = string.IsNullOrWhiteSpace(Note) ? $"[Hoàn tiền]: {reason.Trim()}" : $"{Note}\n[Hoàn tiền]: {reason.Trim()}";
        }

        // Kiểm tra hợp lệ
        private static void Validate(string paymentCode, string accountId, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(paymentCode))
                throw new ValidationException("Mã thanh toán không được để trống.");

            if (string.IsNullOrWhiteSpace(accountId))
                throw new ValidationException("Tài khoản người thanh toán không được để trống.");

            if (amount <= 0)
                throw new ValidationException("Số tiền thanh toán phải lớn hơn 0.");
        }
    }
}
