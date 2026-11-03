using Application.DTOs.Common;

namespace Application.DTOs.Users
{
    // Thông tin nơi cư trú hiện tại của khách thuê
    public class ResidencyInfoDto
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int HouseId { get; set; }
        public string HouseName { get; set; } = string.Empty;
        public string HouseAddress { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }          // Ngày dọn vào ở (StartDate)
        public DateTime ContractEndDate { get; set; }      // Ngày hết hạn hợp đồng (EndDate)
        public decimal RentalPrice { get; set; }           // Tiền thuê/tháng
        public decimal DepositAmount { get; set; }         // Tiền đặt cọc
        public string ContractNumber { get; set; } = string.Empty;
    }

    // 1. DTO Xem thông tin cá nhân (Cả User & Admin đều dùng)
    public class UserProfileResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string CitizenId { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Nếu là khách thuê: kèm thông tin phòng đang ở, trọ nào, ngày vào
        public ResidencyInfoDto? CurrentResidence { get; set; }

        // Nếu là chủ trọ: kèm số lượng dãy trọ đang sở hữu
        public int? OwnedHousesCount { get; set; }
    }

    // 2. DTO Cập nhật thông tin cá nhân
    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string CitizenId { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
    }

    // 3. DTO Đổi mật khẩu cá nhân
    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    // 4. DTO Bộ lọc danh sách người dùng cho Admin
    public class UserFilterRequest
    {
        public string? Keyword { get; set; }        // Tìm theo họ tên, sđt, CCCD, email
        public string? Role { get; set; }           // Lọc theo Role: Tenant, Landlord, Admin
        public bool? IsActive { get; set; }         // Lọc theo trạng thái hoạt động / bị khóa
        public int? HouseId { get; set; }           // Lọc các khách đang ở dãy trọ này
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // 5. DTO Item trong danh sách User dành cho Admin
    public class AdminUserListItemResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string CitizenId { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsLockedPermanently { get; set; }
        public DateTime CreatedAt { get; set; }

        // Thông tin cư trú (ở phòng nào, trọ nào, ngày vào)
        public ResidencyInfoDto? CurrentResidence { get; set; }
    }

    // 6. DTO Chi tiết đầy đủ của User dành cho Admin
    public class AdminUserDetailResponse : UserProfileResponse
    {
        public bool IsLockedPermanently { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public List<ResidencyInfoDto> RentalHistory { get; set; } = new();
    }

    // 7. DTO Cấp lại mật khẩu bởi Admin
    public class AdminResetPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    // 8. DTO Khóa tài khoản
    public class LockUserRequest
    {
        public string? Reason { get; set; }
    }
}
