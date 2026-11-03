using Domain.Exceptions;

namespace Domain.Entities
{
    public class MeterReading
    {
        // Khóa chính
        public int MeterReadingId { get; private set; }

        // Khóa ngoại phòng trọ
        public int RoomId { get; private set; }
        public Room? Room { get; private set; }

        // Khóa ngoại hợp đồng thuê tại thời điểm ghi số (nếu có khách đang ở)
        public int? ContractId { get; private set; }
        public Contract? Contract { get; private set; }

        // Kỳ ghi chỉ số
        public int Month { get; private set; }                     // Tháng ghi số (1 - 12)
        public int Year { get; private set; }                      // Năm ghi số (vd: 2026)
        public DateTime ReadingDate { get; private set; }          // Ngày thực tế chốt số (vd: 30/11/2026)

        // Chỉ số Điện (kWh)
        public decimal OldElectricityIndex { get; private set; }   // Chỉ số điện đầu kỳ (số cũ)
        public decimal NewElectricityIndex { get; private set; }   // Chỉ số điện cuối kỳ (số mới)
        public decimal ElectricityUsage => NewElectricityIndex - OldElectricityIndex; // Số kWh đã dùng

        // Chỉ số Nước (m3)
        public decimal OldWaterIndex { get; private set; }         // Chỉ số nước đầu kỳ (số cũ)
        public decimal NewWaterIndex { get; private set; }         // Chỉ số nước cuối kỳ (số mới)
        public decimal WaterUsage => NewWaterIndex - OldWaterIndex; // Số m3 đã dùng

        // Hình ảnh chụp đồng hồ làm bằng chứng minh bạch (tránh tranh cãi với khách)
        public string? ElectricityImageUrl { get; private set; }
        public string? WaterImageUrl { get; private set; }

        // Trạng thái lập hóa đơn: Nếu đã lập hóa đơn thì khóa không cho sửa tùy tiện
        public bool IsBilled { get; private set; } = false;
        public DateTime? BilledAt { get; private set; }

        // Ghi chú và người ghi
        public string? Note { get; private set; }
        public string? RecordedByAccountId { get; private set; }    // Mã tài khoản người ghi số

        // Constructor cho EF Core
        private MeterReading() { }

        // Constructor nghiệp vụ
        public MeterReading(
            int roomId,
            int month,
            int year,
            DateTime readingDate,
            decimal oldElectricityIndex,
            decimal newElectricityIndex,
            decimal oldWaterIndex,
            decimal newWaterIndex,
            int? contractId = null,
            string? recordedByAccountId = null,
            string? electricityImageUrl = null,
            string? waterImageUrl = null,
            string? note = null)
        {
            Validate(roomId, month, year, oldElectricityIndex, newElectricityIndex, oldWaterIndex, newWaterIndex);

            RoomId = roomId;
            Month = month;
            Year = year;
            ReadingDate = readingDate;
            OldElectricityIndex = oldElectricityIndex;
            NewElectricityIndex = newElectricityIndex;
            OldWaterIndex = oldWaterIndex;
            NewWaterIndex = newWaterIndex;
            ContractId = contractId;
            RecordedByAccountId = recordedByAccountId;
            ElectricityImageUrl = electricityImageUrl?.Trim();
            WaterImageUrl = waterImageUrl?.Trim();
            Note = note?.Trim();
            IsBilled = false;
        }

        // Cập nhật lại chỉ số (chỉ cho phép khi chưa xuất hóa đơn tháng này)
        public void UpdateIndices(
            decimal newElectricityIndex,
            decimal newWaterIndex,
            decimal? oldElectricityIndex = null,
            decimal? oldWaterIndex = null,
            string? note = null)
        {
            if (IsBilled)
                throw new ValidationException("Chỉ số này đã được tạo hóa đơn, không thể tùy ý chỉnh sửa.");

            decimal oldElec = oldElectricityIndex ?? OldElectricityIndex;
            decimal oldWater = oldWaterIndex ?? OldWaterIndex;

            ValidateIndices(oldElec, newElectricityIndex, oldWater, newWaterIndex);

            OldElectricityIndex = oldElec;
            NewElectricityIndex = newElectricityIndex;
            OldWaterIndex = oldWater;
            NewWaterIndex = newWaterIndex;

            if (note != null)
                Note = note.Trim();
        }

        // Đính kèm hoặc cập nhật hình ảnh chụp công tơ
        public void AttachProofImages(string? electricityImageUrl, string? waterImageUrl)
        {
            ElectricityImageUrl = electricityImageUrl?.Trim();
            WaterImageUrl = waterImageUrl?.Trim();
        }

        // Đánh dấu đã lập hóa đơn tiền phòng/dịch vụ
        public void MarkAsBilled()
        {
            IsBilled = true;
            BilledAt = DateTime.UtcNow;
        }

        // Mở khóa nếu xóa/hủy hóa đơn cần tính lại
        public void UnmarkAsBilled()
        {
            IsBilled = false;
            BilledAt = null;
        }

        // Hàm kiểm tra hợp lệ
        private static void Validate(
            int roomId,
            int month,
            int year,
            decimal oldElectricity,
            decimal newElectricity,
            decimal oldWater,
            decimal newWater)
        {
            if (roomId <= 0)
                throw new ValidationException("Mã phòng không hợp lệ.");

            if (month < 1 || month > 12)
                throw new ValidationException("Tháng ghi số phải từ tháng 1 đến tháng 12.");

            if (year < 2000 || year > 2100)
                throw new ValidationException("Năm ghi số không hợp lệ.");

            ValidateIndices(oldElectricity, newElectricity, oldWater, newWater);
        }

        private static void ValidateIndices(
            decimal oldElectricity,
            decimal newElectricity,
            decimal oldWater,
            decimal newWater)
        {
            if (oldElectricity < 0 || newElectricity < 0)
                throw new ValidationException("Chỉ số điện không được là số âm.");

            if (newElectricity < oldElectricity)
                throw new ValidationException($"Chỉ số điện mới ({newElectricity}) không được nhỏ hơn chỉ số điện cũ ({oldElectricity}).");

            if (oldWater < 0 || newWater < 0)
                throw new ValidationException("Chỉ số nước không được là số âm.");

            if (newWater < oldWater)
                throw new ValidationException($"Chỉ số nước mới ({newWater}) không được nhỏ hơn chỉ số nước cũ ({oldWater}).");
        }
    }
}
