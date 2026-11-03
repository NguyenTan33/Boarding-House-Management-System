using Domain.Exceptions;

namespace Domain.Entities
{
    public class House
    {
        // Khóa chính (Primary Key - tự tăng trong Database)
        public int HouseId { get; private set; }

        // Người quản lý / Chủ nhà (liên kết với Account/IdentityUser nếu có)
        public string? OwnerId { get; private set; }

        // Thông tin cơ bản
        public string HouseName { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;
        public string? Description { get; private set; }

        // Đơn giá dịch vụ áp dụng chung cho cả nhà trọ (VNĐ)
        public decimal ElectricityUnitPrice { get; private set; } // Giá điện (vd: 3500 đ/kWh)
        public decimal WaterUnitPrice { get; private set; }       // Giá nước (vd: 20000 đ/m3 hoặc đ/người)
        public decimal InternetUnitPrice { get; private set; }    // Tiền mạng/Wifi hàng tháng (vd: 80000 đ/phòng)
        public decimal CleaningUnitPrice { get; private set; }    // Phí vệ sinh, rác hàng tháng (vd: 30000 đ/phòng)
        public decimal ParkingUnitPrice { get; private set; }     // Phí giữ xe hàng tháng (vd: 120000 đ/xe)

        // Trạng thái hoạt động
        public bool IsActive { get; private set; } = true;

        // Quan hệ 1 - N: Một nhà trọ có nhiều phòng
        public ICollection<Room> Rooms { get; private set; } = new List<Room>();

        // Constructor dành cho EF Core
        private House() { }

        // Constructor nghiệp vụ
        public House(
            string houseName,
            string address,
            decimal electricityUnitPrice,
            decimal waterUnitPrice,
            decimal internetUnitPrice = 0,
            decimal cleaningUnitPrice = 0,
            decimal parkingUnitPrice = 0,
            string? description = null,
            string? ownerId = null)
        {
            Validate(houseName, address, electricityUnitPrice, waterUnitPrice, internetUnitPrice, cleaningUnitPrice, parkingUnitPrice);

            HouseName = houseName.Trim();
            Address = address.Trim();
            ElectricityUnitPrice = electricityUnitPrice;
            WaterUnitPrice = waterUnitPrice;
            InternetUnitPrice = internetUnitPrice;
            CleaningUnitPrice = cleaningUnitPrice;
            ParkingUnitPrice = parkingUnitPrice;
            Description = description?.Trim();
            OwnerId = ownerId;
            IsActive = true;
        }

        // Cập nhật thông tin cơ bản nhà trọ
        public void UpdateInfo(string houseName, string address, string? description)
        {
            if (string.IsNullOrWhiteSpace(houseName))
                throw new ValidationException("Tên nhà trọ không được để trống.");

            if (string.IsNullOrWhiteSpace(address))
                throw new ValidationException("Địa chỉ nhà trọ không được để trống.");

            HouseName = houseName.Trim();
            Address = address.Trim();
            Description = description?.Trim();
        }

        // Cập nhật toàn bộ biểu phí dịch vụ
        public void UpdateServicePrices(
            decimal electricityUnitPrice,
            decimal waterUnitPrice,
            decimal internetUnitPrice,
            decimal cleaningUnitPrice,
            decimal parkingUnitPrice)
        {
            ValidatePrices(electricityUnitPrice, waterUnitPrice, internetUnitPrice, cleaningUnitPrice, parkingUnitPrice);

            ElectricityUnitPrice = electricityUnitPrice;
            WaterUnitPrice = waterUnitPrice;
            InternetUnitPrice = internetUnitPrice;
            CleaningUnitPrice = cleaningUnitPrice;
            ParkingUnitPrice = parkingUnitPrice;
        }

        // Cập nhật riêng từng loại phí khi có thay đổi đơn lẻ
        public void UpdateElectricityPrice(decimal price)
        {
            if (price < 0) throw new ValidationException("Đơn giá điện không được là số âm.");
            ElectricityUnitPrice = price;
        }

        public void UpdateWaterPrice(decimal price)
        {
            if (price < 0) throw new ValidationException("Đơn giá nước không được là số âm.");
            WaterUnitPrice = price;
        }

        public void UpdateInternetPrice(decimal price)
        {
            if (price < 0) throw new ValidationException("Đơn giá internet/wifi không được là số âm.");
            InternetUnitPrice = price;
        }

        public void UpdateCleaningPrice(decimal price)
        {
            if (price < 0) throw new ValidationException("Phí vệ sinh không được là số âm.");
            CleaningUnitPrice = price;
        }

        public void UpdateParkingPrice(decimal price)
        {
            if (price < 0) throw new ValidationException("Phí giữ xe không được là số âm.");
            ParkingUnitPrice = price;
        }

        // Kích hoạt nhà trọ
        public void Activate()
        {
            IsActive = true;
        }

        // Vô hiệu hóa (tạm ngừng quản lý nhà trọ này)
        public void Deactivate()
        {
            IsActive = false;
        }

        // Hàm kiểm tra hợp lệ
        private static void Validate(
            string houseName,
            string address,
            decimal electricityUnitPrice,
            decimal waterUnitPrice,
            decimal internetUnitPrice,
            decimal cleaningUnitPrice,
            decimal parkingUnitPrice)
        {
            if (string.IsNullOrWhiteSpace(houseName))
                throw new ValidationException("Tên nhà trọ không được để trống.");

            if (string.IsNullOrWhiteSpace(address))
                throw new ValidationException("Địa chỉ nhà trọ không được để trống.");

            ValidatePrices(electricityUnitPrice, waterUnitPrice, internetUnitPrice, cleaningUnitPrice, parkingUnitPrice);
        }

        private static void ValidatePrices(
            decimal electricityUnitPrice,
            decimal waterUnitPrice,
            decimal internetUnitPrice,
            decimal cleaningUnitPrice,
            decimal parkingUnitPrice)
        {
            if (electricityUnitPrice < 0)
                throw new ValidationException("Đơn giá điện không được là số âm.");

            if (waterUnitPrice < 0)
                throw new ValidationException("Đơn giá nước không được là số âm.");

            if (internetUnitPrice < 0)
                throw new ValidationException("Đơn giá internet/wifi không được là số âm.");

            if (cleaningUnitPrice < 0)
                throw new ValidationException("Phí vệ sinh không được là số âm.");

            if (parkingUnitPrice < 0)
                throw new ValidationException("Phí giữ xe không được là số âm.");
        }
    }
}
