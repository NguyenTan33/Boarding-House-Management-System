using Application.DTOs.Common;
using Application.DTOs.Users;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        // 1. Chức năng cá nhân (User & Admin đều có thể chỉnh sửa thông tin của mình)
        Task<UserProfileResponse> GetCurrentUserProfileAsync(string userId);
        Task UpdateCurrentUserProfileAsync(string userId, UpdateProfileRequest request);
        Task ChangePasswordAsync(string userId, ChangePasswordRequest request);

        // 2. Chức năng quản lý User dành riêng cho Admin
        Task<PagedResult<AdminUserListItemResponse>> GetUsersAsync(UserFilterRequest filter);
        Task<AdminUserDetailResponse> GetUserDetailAsync(string targetUserId);
        Task LockUserPermanentlyAsync(string targetUserId, string currentAdminId, string? reason = null);
        Task UnlockUserAsync(string targetUserId, string currentAdminId);
        Task ResetUserPasswordAsync(string targetUserId, AdminResetPasswordRequest request, string currentAdminId);
    }
}
