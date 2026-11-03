using Domain.Enum;
using Domain.Exceptions;

namespace Domain.Entities
{
    public class Invoice
    {
        // Khóa chính
        public int InvoiceId { get; private set; }

        // Mã hóa đơn (vd: "HDT-202611-P101", phục vụ xuất hóa đơn, gửi Zalo, QR code)
        public string InvoiceCode { get; private set; } = string.Empty;

        // Các liên kết quan hệ
        public int ContractId { get; private set; }
        public Contract? Contract { get; private set; }

        public int RoomId { get; private set; }
        public Room? Room { get; private set; }

        public string AccountId { get; private set; } = string.Empty; // Khách thuê nhận hóa đơn
        public Account? Account { get; private set; }

        public int? MeterReadingId { get; private set; }              // Chỉ số điện nước tương ứng của tháng này
        public MeterReading? MeterReading { get; private set; }

        // Kỳ thanh toán
        public int Month { get; private set; }                        // Tháng tính tiền (1 - 12)
        public int Year { get; private set; }                         // Năm tính tiền (vd: 2026)
        public DateTime IssueDate { get; private set; }               // Ngày lập hóa đơn
        public DateTime DueDate { get; private set; }                 // Hạn chót thanh toán (vd: ngày 05 hàng tháng)
        public DateTime? PaymentDate { get; private set; }            // Ngày thanh toán hoàn tất

        // Tiền phòng tháng này
        public decimal RoomPrice { get; private set; }

        // Tiền Điện: Lượng dùng (kWh) * Đơn giá
        public decimal ElectricityUsage { get; private set; }
        public decimal ElectricityUnitPrice { get; private set; }
        public decimal ElectricityCost { get; private set; }

        // Tiền Nước: Lượng dùng (m3 hoặc số người) * Đơn giá
        public decimal WaterUsage { get; private set; }
        public decimal WaterUnitPrice { get; private set; }
        public decimal WaterCost { get; private set; }

        // Dịch vụ cố định
        public decimal InternetCost { get; private set; }             // Tiền wifi/mạng
        public decimal CleaningCost { get; private set; }             // Phí rác, vệ sinh chung

        // Tiền giữ xe: Số lượng xe * Đơn giá
        public int NumberOfVehicles { get; private set; }
        public decimal ParkingUnitPrice { get; private set; }
        public decimal ParkingCost { get; private set; }

        // Chi phí phát sinh khác (hư hỏng đồ, nợ tháng trước...)
        public decimal OtherFees { get; private set; } = 0;
        public string? OtherFeesDescription { get; private set; }

        // Giảm trừ / Khuyến mãi
        public decimal DiscountAmount { get; private set; } = 0;
        public string? DiscountReason { get; private set; }

        // Tổng tiền & Thanh toán
        public decimal TotalAmount { get; private set; }              // Tổng tiền cần trả
        public decimal PaidAmount { get; private set; } = 0;          // Số tiền đã trả
        public decimal RemainingAmount => Math.Max(0, TotalAmount - PaidAmount); // Số tiền còn thiếu

        // Trạng thái và phương thức thanh toán
        public InvoiceStatus Status { get; private set; } = InvoiceStatus.Unpaid;
        public string? PaymentMethod { get; private set; }            // Tiền mặt / Chuyển khoản / VietQR
        public string? Note { get; private set; }                     // Ghi chú hóa đơn

        // Lịch sử các lần giao dịch thanh toán cho hóa đơn này
        public ICollection<Payment> Payments { get; private set; } = new List<Payment>();

        // Constructor cho EF Core
        private Invoice() { }

        // Constructor nghiệp vụ
        public Invoice(
            string invoiceCode,
            int contractId,
            int roomId,
            string accountId,
            int month,
            int year,
            DateTime issueDate,
            DateTime dueDate,
            decimal roomPrice,
            decimal electricityUsage,
            decimal electricityUnitPrice,
            decimal waterUsage,
            decimal waterUnitPrice,
            decimal internetCost = 0,
            decimal cleaningCost = 0,
            int numberOfVehicles = 0,
            decimal parkingUnitPrice = 0,
            decimal otherFees = 0,
            string? otherFeesDescription = null,
            decimal discountAmount = 0,
            string? discountReason = null,
            int? meterReadingId = null,
            string? note = null)
        {
            Validate(invoiceCode, contractId, roomId, accountId, month, year, issueDate, dueDate, roomPrice);

            InvoiceCode = invoiceCode.Trim();
            ContractId = contractId;
            RoomId = roomId;
            AccountId = accountId;
            MeterReadingId = meterReadingId;
            Month = month;
            Year = year;
            IssueDate = issueDate;
            DueDate = dueDate;
            RoomPrice = roomPrice;

            ElectricityUsage = electricityUsage < 0 ? 0 : electricityUsage;
            ElectricityUnitPrice = electricityUnitPrice < 0 ? 0 : electricityUnitPrice;
            ElectricityCost = ElectricityUsage * ElectricityUnitPrice;

            WaterUsage = waterUsage < 0 ? 0 : waterUsage;
            WaterUnitPrice = waterUnitPrice < 0 ? 0 : waterUnitPrice;
            WaterCost = WaterUsage * WaterUnitPrice;

            InternetCost = internetCost < 0 ? 0 : internetCost;
            CleaningCost = cleaningCost < 0 ? 0 : cleaningCost;

            NumberOfVehicles = numberOfVehicles < 0 ? 0 : numberOfVehicles;
            ParkingUnitPrice = parkingUnitPrice < 0 ? 0 : parkingUnitPrice;
            ParkingCost = NumberOfVehicles * ParkingUnitPrice;

            OtherFees = otherFees < 0 ? 0 : otherFees;
            OtherFeesDescription = otherFeesDescription?.Trim();

            DiscountAmount = discountAmount < 0 ? 0 : discountAmount;
            DiscountReason = discountReason?.Trim();

            Note = note?.Trim();
            PaidAmount = 0;
            Status = InvoiceStatus.Unpaid;

            RecalculateTotal();
        }

        // Ghi nhận thanh toán (có thể trả hết hoặc trả góp một phần)
        public void RecordPayment(decimal amount, string paymentMethod, string? note = null)
        {
            if (Status == InvoiceStatus.Paid)
                throw new ValidationException("Hóa đơn này đã được thanh toán đầy đủ.");

            if (Status == InvoiceStatus.Cancelled)
                throw new ValidationException("Hóa đơn đã bị hủy, không thể ghi nhận thanh toán.");

            if (amount <= 0)
                throw new ValidationException("Số tiền thanh toán phải lớn hơn 0.");

            if (amount > RemainingAmount)
                throw new ValidationException($"Số tiền thanh toán ({amount:N0} đ) vượt quá số tiền còn nợ ({RemainingAmount:N0} đ).");

            PaidAmount += amount;
            PaymentMethod = paymentMethod.Trim();

            if (PaidAmount >= TotalAmount)
            {
                Status = InvoiceStatus.Paid;
                PaymentDate = DateTime.UtcNow;
            }
            else
            {
                Status = InvoiceStatus.PartiallyPaid;
            }

            if (!string.IsNullOrWhiteSpace(note))
            {
                Note = string.IsNullOrWhiteSpace(Note) ? note.Trim() : $"{Note}\n{note.Trim()}";
            }
        }

        // Thêm chi phí phát sinh (vd: tiền sửa vòi nước, phí phạt vệ sinh...)
        public void AddOtherFee(decimal fee, string description)
        {
            if (Status == InvoiceStatus.Paid)
                throw new ValidationException("Không thể thêm phụ phí vào hóa đơn đã thanh toán.");

            if (fee <= 0)
                throw new ValidationException("Chi phí phát sinh phải lớn hơn 0.");

            OtherFees += fee;
            OtherFeesDescription = string.IsNullOrWhiteSpace(OtherFeesDescription) 
                ? description.Trim() 
                : $"{OtherFeesDescription}; {description.Trim()}";

            RecalculateTotal();
        }

        // Áp dụng giảm giá / chiết khấu
        public void ApplyDiscount(decimal discountAmount, string reason)
        {
            if (Status == InvoiceStatus.Paid)
                throw new ValidationException("Không thể áp dụng giảm giá cho hóa đơn đã thanh toán.");

            if (discountAmount <= 0)
                throw new ValidationException("Số tiền giảm giá phải lớn hơn 0.");

            DiscountAmount = discountAmount;
            DiscountReason = reason.Trim();

            RecalculateTotal();
        }

        // Kiểm tra và đánh dấu quá hạn nếu chưa trả xong khi đã qua DueDate
        public void CheckOverdue()
        {
            if (Status != InvoiceStatus.Paid && Status != InvoiceStatus.Cancelled && DateTime.UtcNow > DueDate)
            {
                Status = InvoiceStatus.Overdue;
            }
        }

        // Hủy hóa đơn
        public void Cancel(string reason)
        {
            if (PaidAmount > 0)
                throw new ValidationException("Hóa đơn đã có khoản thanh toán, cần hoàn tiền trước khi hủy.");

            Status = InvoiceStatus.Cancelled;
            Note = string.IsNullOrWhiteSpace(Note) ? $"[Hủy]: {reason}" : $"{Note}\n[Hủy]: {reason}";
        }

        // Tính lại tổng tiền
        private void RecalculateTotal()
        {
            decimal sum = RoomPrice + ElectricityCost + WaterCost + InternetCost + CleaningCost + ParkingCost + OtherFees - DiscountAmount;
            TotalAmount = sum < 0 ? 0 : sum;
        }

        // Kiểm tra hợp lệ
        private static void Validate(
            string invoiceCode,
            int contractId,
            int roomId,
            string accountId,
            int month,
            int year,
            DateTime issueDate,
            DateTime dueDate,
            decimal roomPrice)
        {
            if (string.IsNullOrWhiteSpace(invoiceCode))
                throw new ValidationException("Mã hóa đơn không được để trống.");

            if (contractId <= 0)
                throw new ValidationException("Hợp đồng thuê không hợp lệ.");

            if (roomId <= 0)
                throw new ValidationException("Phòng thuê không hợp lệ.");

            if (string.IsNullOrWhiteSpace(accountId))
                throw new ValidationException("Tài khoản người thanh toán không được để trống.");

            if (month < 1 || month > 12)
                throw new ValidationException("Tháng hóa đơn phải từ 1 đến 12.");

            if (year < 2000 || year > 2100)
                throw new ValidationException("Năm hóa đơn không hợp lệ.");

            if (dueDate < issueDate)
                throw new ValidationException("Hạn thanh toán không được trước ngày lập hóa đơn.");

            if (roomPrice <= 0)
                throw new ValidationException("Tiền thuê phòng phải lớn hơn 0.");
        }
    }
}
