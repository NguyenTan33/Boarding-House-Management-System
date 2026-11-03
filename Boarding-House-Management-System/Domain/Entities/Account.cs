using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class Account : IdentityUser
    {
        // Thông tin cá nhân khách thuê / chủ trọ
        public string FullName { get; private set; } = string.Empty;
        public string CitizenId { get; private set; } = string.Empty;   // Số CCCD / CMND
        public DateTime? DateOfBirth { get; private set; }              // Ngày sinh
        public string? Gender { get; private set; }                     // Giới tính (Nam / Nữ / Khác)
        public string? Address { get; private set; }                    // Địa chỉ thường trú / Quê quán
        public string? AvatarUrl { get; private set; }                  // Ảnh đại diện

        // Trạng thái tài khoản & Thời gian tạo
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        // Quan hệ 1 - N: Một tài khoản có thể có nhiều hợp đồng thuê
        public ICollection<Contract> Contracts { get; private set; } = new List<Contract>();

        // Danh sách các hóa đơn tiền trọ cần thanh toán của tài khoản này
        public ICollection<Invoice> Invoices { get; private set; } = new List<Invoice>();

        // Quan hệ 1 - N: Nếu tài khoản là chủ trọ, có thể sở hữu nhiều dãy trọ
        public ICollection<House> Houses { get; private set; } = new List<House>();

        // Constructor cho Identity / EF Core
        public Account() { }

        // Constructor nghiệp vụ
        public Account(
            string userName, 
            string email, 
            string fullName, 
            string citizenId, 
            string? phoneNumber = null,
            DateTime? dateOfBirth = null,
            string? address = null)
        {
            Validate(fullName, citizenId);

            UserName = userName.Trim();
            Email = email.Trim();
            FullName = fullName.Trim();
            CitizenId = citizenId.Trim();
            PhoneNumber = phoneNumber?.Trim();
            DateOfBirth = dateOfBirth;
            Address = address?.Trim();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        // Cập nhật thông tin cá nhân
        public void UpdateProfile(
            string fullName, 
            string citizenId, 
            string? phoneNumber, 
            DateTime? dateOfBirth, 
            string? address,
            string? gender = null,
            string? avatarUrl = null)
        {
            Validate(fullName, citizenId);

            FullName = fullName.Trim();
            CitizenId = citizenId.Trim();
            PhoneNumber = phoneNumber?.Trim();
            DateOfBirth = dateOfBirth;
            Address = address?.Trim();
            Gender = gender?.Trim();
            AvatarUrl = avatarUrl?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        // Kích hoạt tài khoản
        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        // Vô hiệu hóa tài khoản
        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        private static void Validate(string fullName, string citizenId)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ValidationException("Họ và tên không được để trống.");

            if (string.IsNullOrWhiteSpace(citizenId))
                throw new ValidationException("Số CCCD/CMND không được để trống.");
        }
    }
}
