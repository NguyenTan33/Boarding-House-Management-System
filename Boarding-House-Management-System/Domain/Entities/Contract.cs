using Domain.Enum;
using Domain.Exceptions;

namespace Domain.Entities
{
    public class Contract
    {
        // Khóa chính
        public int ContractId { get; private set; }

        // Mã số hợp đồng (vd: "HD-202611-001", phục vụ in ấn, tra cứu)
        public string ContractNumber { get; private set; } = string.Empty;

        // Khóa ngoại phòng thuê
        public int RoomId { get; private set; }
        public Room? Room { get; private set; }

        // Khóa ngoại tài khoản khách thuê đứng tên hợp đồng
        public string AccountId { get; private set; } = string.Empty;
        public Account? Account { get; private set; }

        // Thông tin người ở và xe cộ của hợp đồng này
        public int NumberOfRenters { get; private set; } = 1;              // Số người ở thực tế trong phòng
        public int NumberOfVehicles { get; private set; } = 0;             // Số lượng xe gửi

        // Thời hạn hợp đồng
        public DateTime StartDate { get; private set; }                    // Ngày bắt đầu hợp đồng
        public DateTime EndDate { get; private set; }                      // Ngày kết thúc theo thỏa thuận
        public DateTime? ActualEndDate { get; private set; }               // Ngày trả phòng/thanh lý thực tế

        // Tiền thuê & Tiền cọc
        public decimal RentalPrice { get; private set; }                   // Giá thuê phòng/tháng tại thời điểm ký
        public decimal DepositAmount { get; private set; }                 // Tiền đặt cọc
        public decimal? DepositRefunded { get; private set; }              // Tiền cọc thực tế hoàn trả khi trả phòng
        public int PaymentCycleMonths { get; private set; } = 1;           // Kỳ thanh toán (mặc định 1 tháng/lần)

        // Chỉ số công tơ ban đầu lúc bàn giao phòng
        public decimal InitialElectricityIndex { get; private set; }       // Chỉ số điện ban đầu (kWh)
        public decimal InitialWaterIndex { get; private set; }             // Chỉ số nước ban đầu (m3)

        // Đơn giá dịch vụ đã chốt trong hợp đồng
        public decimal ElectricityUnitPrice { get; private set; }          // Giá điện (đ/kWh)
        public decimal WaterUnitPrice { get; private set; }                // Giá nước (đ/m3 hoặc đ/người)
        public decimal InternetUnitPrice { get; private set; }             // Tiền wifi/tháng
        public decimal CleaningUnitPrice { get; private set; }             // Phí rác, vệ sinh/tháng
        public decimal ParkingUnitPrice { get; private set; }              // Phí gửi xe/chiếc/tháng

        // Trạng thái hợp đồng
        public ContractStatus Status { get; private set; } = ContractStatus.Draft;
        public string? Note { get; private set; }                          // Ghi chú / điều khoản riêng

        // Hình ảnh / Bản scan hợp đồng giấy đã ký (ảnh chụp hoặc file PDF)
        public string? ContractImageUrl { get; private set; }

        // Danh sách các kỳ chốt chỉ số điện nước trong thời gian hợp đồng
        public ICollection<MeterReading> MeterReadings { get; private set; } = new List<MeterReading>();

        // Danh sách các hóa đơn tiền phòng hàng tháng của hợp đồng này
        public ICollection<Invoice> Invoices { get; private set; } = new List<Invoice>();

        // Danh sách các giao dịch thanh toán liên quan đến hợp đồng này (đặt cọc, tiền phòng...)
        public ICollection<Payment> Payments { get; private set; } = new List<Payment>();

        // Constructor cho EF Core
        private Contract() { }

        // Constructor nghiệp vụ
        public Contract(
            string contractNumber,
            int roomId,
            string accountId,
            DateTime startDate,
            DateTime endDate,
            decimal rentalPrice,
            decimal depositAmount,
            decimal initialElectricityIndex,
            decimal initialWaterIndex,
            decimal electricityUnitPrice,
            decimal waterUnitPrice,
            decimal internetUnitPrice = 0,
            decimal cleaningUnitPrice = 0,
            decimal parkingUnitPrice = 0,
            int numberOfRenters = 1,
            int numberOfVehicles = 0,
            int paymentCycleMonths = 1,
            string? note = null)
        {
            Validate(contractNumber, roomId, accountId, startDate, endDate, rentalPrice, depositAmount);

            ContractNumber = contractNumber.Trim();
            RoomId = roomId;
            AccountId = accountId;
            StartDate = startDate;
            EndDate = endDate;
            RentalPrice = rentalPrice;
            DepositAmount = depositAmount;
            InitialElectricityIndex = initialElectricityIndex;
            InitialWaterIndex = initialWaterIndex;
            ElectricityUnitPrice = electricityUnitPrice;
            WaterUnitPrice = waterUnitPrice;
            InternetUnitPrice = internetUnitPrice;
            CleaningUnitPrice = cleaningUnitPrice;
            ParkingUnitPrice = parkingUnitPrice;
            NumberOfRenters = numberOfRenters < 1 ? 1 : numberOfRenters;
            NumberOfVehicles = numberOfVehicles < 0 ? 0 : numberOfVehicles;
            PaymentCycleMonths = paymentCycleMonths < 1 ? 1 : paymentCycleMonths;
            Note = note?.Trim();
            Status = ContractStatus.Draft;
        }

        // Kích hoạt hợp đồng khi khách chính thức nhận phòng và dọn vào
        public void Activate(decimal? initialElectricity = null, decimal? initialWater = null)
        {
            if (Status != ContractStatus.Draft)
                throw new ValidationException("Chỉ hợp đồng ở trạng thái nháp mới có thể kích hoạt.");

            if (initialElectricity.HasValue)
            {
                if (initialElectricity.Value < 0) throw new ValidationException("Chỉ số điện không được âm.");
                InitialElectricityIndex = initialElectricity.Value;
            }

            if (initialWater.HasValue)
            {
                if (initialWater.Value < 0) throw new ValidationException("Chỉ số nước không được âm.");
                InitialWaterIndex = initialWater.Value;
            }

            Status = ContractStatus.Active;
        }

        // Gia hạn hợp đồng thêm thời gian
        public void Renew(DateTime newEndDate, decimal? newRentalPrice = null)
        {
            if (Status != ContractStatus.Active && Status != ContractStatus.PendingRenewal)
                throw new ValidationException("Chỉ hợp đồng đang hoạt động hoặc sắp hết hạn mới được gia hạn.");

            if (newEndDate <= EndDate)
                throw new ValidationException("Ngày hết hạn mới phải lớn hơn ngày hết hạn hiện tại.");

            EndDate = newEndDate;

            if (newRentalPrice.HasValue)
            {
                if (newRentalPrice.Value <= 0)
                    throw new ValidationException("Giá thuê mới phải lớn hơn 0.");
                RentalPrice = newRentalPrice.Value;
            }

            Status = ContractStatus.Active;
        }

        // Thanh lý / Trả phòng kết thúc hợp đồng
        public void Terminate(DateTime actualEndDate, decimal refundedDeposit, string? returnNote = null)
        {
            if (Status != ContractStatus.Active && Status != ContractStatus.PendingRenewal)
                throw new ValidationException("Chỉ hợp đồng đang hoạt động mới có thể thanh lý.");

            if (actualEndDate < StartDate)
                throw new ValidationException("Ngày thanh lý thực tế không được trước ngày bắt đầu hợp đồng.");

            if (refundedDeposit < 0 || refundedDeposit > DepositAmount)
                throw new ValidationException($"Số tiền hoàn cọc phải từ 0 đến {DepositAmount:N0} VNĐ.");

            ActualEndDate = actualEndDate;
            DepositRefunded = refundedDeposit;
            Status = ContractStatus.Terminated;

            if (!string.IsNullOrWhiteSpace(returnNote))
            {
                Note = string.IsNullOrWhiteSpace(Note) 
                    ? $"[Thanh lý]: {returnNote.Trim()}" 
                    : $"{Note}\n[Thanh lý]: {returnNote.Trim()}";
            }
        }

        // Hủy hợp đồng trước khi dọn vào
        public void Cancel(string reason)
        {
            if (Status != ContractStatus.Draft)
                throw new ValidationException("Chỉ có thể hủy hợp đồng khi chưa kích hoạt/chưa dọn vào.");

            Status = ContractStatus.Cancelled;
            Note = string.IsNullOrWhiteSpace(Note) ? $"[Hủy]: {reason}" : $"{Note}\n[Hủy]: {reason}";
        }

        // Cập nhật số người ở và số lượng xe máy của phòng
        public void UpdateOccupants(int numberOfRenters, int numberOfVehicles)
        {
            if (numberOfRenters < 1)
                throw new ValidationException("Số người ở phải từ 1 người trở lên.");

            if (numberOfVehicles < 0)
                throw new ValidationException("Số lượng xe không được âm.");

            NumberOfRenters = numberOfRenters;
            NumberOfVehicles = numberOfVehicles;
        }

        // Chuyển nhượng / Đổi tài khoản đứng tên hợp đồng
        public void TransferAccount(string newAccountId)
        {
            if (string.IsNullOrWhiteSpace(newAccountId))
                throw new ValidationException("Mã tài khoản mới không được để trống.");

            AccountId = newAccountId;
        }

        // Cập nhật lại chỉ số ban đầu lúc bàn giao phòng (nếu có sai sót lúc chốt biên bản)
        public void UpdateInitialIndices(decimal electricityIndex, decimal waterIndex)
        {
            if (electricityIndex < 0) throw new ValidationException("Chỉ số điện ban đầu không được âm.");
            if (waterIndex < 0) throw new ValidationException("Chỉ số nước ban đầu không được âm.");

            InitialElectricityIndex = electricityIndex;
            InitialWaterIndex = waterIndex;
        }

        // Hàm kiểm tra hợp lệ
        private static void Validate(
            string contractNumber,
            int roomId,
            string accountId,
            DateTime startDate,
            DateTime endDate,
            decimal rentalPrice,
            decimal depositAmount)
        {
            if (string.IsNullOrWhiteSpace(contractNumber))
                throw new ValidationException("Mã số hợp đồng không được để trống.");

            if (roomId <= 0)
                throw new ValidationException("Phòng thuê không hợp lệ.");

            if (string.IsNullOrWhiteSpace(accountId))
                throw new ValidationException("Mã tài khoản khách thuê (AccountId) không được để trống.");

            if (endDate <= startDate)
                throw new ValidationException("Ngày kết thúc hợp đồng phải sau ngày bắt đầu.");

            if (rentalPrice <= 0)
                throw new ValidationException("Giá thuê phòng phải lớn hơn 0.");

            if (depositAmount < 0)
                throw new ValidationException("Tiền đặt cọc không được là số âm.");
        }
    }
}
