using Domain.Enum;
using Domain.Exceptions;

namespace Domain.Entities
{
    public class Room
    {
        // Khóa chính (Primary Key - tự tăng trong Database)
        public int RoomId { get; private set; }

        // Khóa ngoại liên kết tới House
        public int HouseId { get; private set; };
        public House? House { get; private set; }

        // Thông tin phòng
        public string RoomNumber { get; private set; } = string.Empty; // Số/tên phòng: "101", "201", "P.102"...
        public decimal Price { get; private set; }                    // Tiền tệ dùng decimal
        public int Floor { get; private set; }                        // Tầng (0 là tầng trệt)
        public double Size { get; private set; }                      // Diện tích (m2)
        public int MaxRenters { get; private set; }                   // Số người ở tối đa
        public bool HasBalcony { get; private set; } = false;         // Có ban công không

        // Trạng thái phòng
        public RoomStatus Status { get; private set; } = RoomStatus.Available; // Trống, Đang thuê, Đã cọc...
        public bool IsActive { get; private set; } = true;                     // Đang hoạt động / Khóa phòng

        // Constructor cho ORM (EF Core)
        private Room() { }

        // Constructor nghiệp vụ
        public Room(
            int houseId,
            string roomNumber,
            decimal price,
            int floor,
            double size,
            int maxRenters,
            bool hasBalcony = false,
            bool isActive = true)
        {
            Validate(roomNumber, price, floor, size, maxRenters);

            HouseId = houseId;
            RoomNumber = roomNumber.Trim();
            Price = price;
            Floor = floor;
            Size = size;
            MaxRenters = maxRenters;
            HasBalcony = hasBalcony;
            Status = RoomStatus.Available;
            IsActive = isActive;
        }

        // Cập nhật thông tin tổng thể của phòng
        public void UpdateRoom(
            string roomNumber,
            decimal price,
            int floor,
            double size,
            int maxRenters,
            bool hasBalcony,
            bool isActive)
        {
            Validate(roomNumber, price, floor, size, maxRenters);

            RoomNumber = roomNumber.Trim();
            Price = price;
            Floor = floor;
            Size = size;
            MaxRenters = maxRenters;
            HasBalcony = hasBalcony;
            IsActive = isActive;
        }

        // Cập nhật giá thuê
        public void UpdatePrice(decimal price)
        {
            if (price <= 0)
                throw new ValidationException("Giá phòng phải lớn hơn 0.");

            Price = price;
        }

        // Cập nhật diện tích
        public void UpdateSize(double size)
        {
            if (size <= 0)
                throw new ValidationException("Diện tích phòng phải lớn hơn 0.");

            Size = size;
        }

        // Cập nhật tầng
        public void UpdateFloor(int floor)
        {
            if (floor < 0)
                throw new ValidationException("Tầng không được âm (tầng trệt là 0).");

            Floor = floor;
        }

        // Cập nhật số người tối đa
        public void UpdateMaxRenters(int maxRenters)
        {
            if (maxRenters < 1)
                throw new ValidationException("Số người tối đa phải từ 1 người trở lên.");

            MaxRenters = maxRenters;
        }

        // Cập nhật ban công
        public void SetBalcony(bool hasBalcony)
        {
            HasBalcony = hasBalcony;
        }

        // Đổi trạng thái phòng (Trống, Đang thuê, Đã cọc, Bảo trì)
        public void ChangeStatus(RoomStatus newStatus)
        {
            Status = newStatus;
        }

        // Kích hoạt phòng
        public void Activate()
        {
            IsActive = true;
        }

        // Vô hiệu hóa phòng (tạm ngưng kinh doanh)
        public void Deactivate()
        {
            IsActive = false;
        }

        // Hàm kiểm tra hợp lệ tập trung (tránh lặp code)
        private static void Validate(string roomNumber, decimal price, int floor, double size, int maxRenters)
        {
            if (string.IsNullOrWhiteSpace(roomNumber))
                throw new ValidationException("Số/Tên phòng không được để trống.");

            if (price <= 0)
                throw new ValidationException("Giá phòng phải lớn hơn 0.");

            if (floor < 0)
                throw new ValidationException("Tầng phòng không được âm (tầng trệt là 0).");

            if (size <= 0)
                throw new ValidationException("Diện tích phòng phải lớn hơn 0.");

            if (maxRenters < 1)
                throw new ValidationException("Số người tối đa phải từ 1 người trở lên.");
        }
    }
}
