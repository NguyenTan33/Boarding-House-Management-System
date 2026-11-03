namespace Domain.Enum
{
    public enum RoomStatus
    {
        Available = 1,       // Phòng trống
        Occupied = 2,        // Đang thuê
        Reserved = 3,        // Đã cọc giữ chỗ
        UnderMaintenance = 4 // Đang sửa chữa / bảo trì
    }
}
