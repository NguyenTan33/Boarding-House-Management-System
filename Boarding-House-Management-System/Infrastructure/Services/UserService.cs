using Application.DTOs.Common;
using Application.DTOs.Users;
using Application.Exceptions;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enum;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<Account> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public UserService(
            UserManager<Account> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        #region 1. CHỨC NĂNG CÁ NHÂN (USER & ADMIN)

        public async Task<UserProfileResponse> GetCurrentUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
                throw new NotFoundException("Không tìm thấy thông tin tài khoản người dùng.");

            var roles = await _userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault() ?? "Tenant";

            // Lấy thông tin phòng đang ở (nếu có hợp đồng Active)
            ResidencyInfoDto? currentResidence = null;
            var activeContract = await _context.Contracts
                .Include(c => c.Room)
                    .ThenInclude(r => r!.House)
                .Where(c => c.AccountId == userId && c.Status == ContractStatus.Active)
                .FirstOrDefaultAsync();

            if (activeContract != null)
            {
                currentResidence = new ResidencyInfoDto
                {
                    RoomId = activeContract.RoomId,
                    RoomNumber = activeContract.Room?.RoomNumber ?? string.Empty,
                    HouseId = activeContract.Room?.HouseId ?? 0,
                    HouseName = activeContract.Room?.House?.HouseName ?? string.Empty,
                    HouseAddress = activeContract.Room?.House?.Address ?? string.Empty,
                    CheckInDate = activeContract.StartDate,
                    ContractEndDate = activeContract.EndDate,
                    RentalPrice = activeContract.RentalPrice,
                    DepositAmount = activeContract.DepositAmount,
                    ContractNumber = activeContract.ContractNumber
                };
            }

            int? ownedHousesCount = null;
            if (role == "Landlord" || role == "Admin")
            {
                ownedHousesCount = await _context.Houses.CountAsync(h => h.OwnerId == userId && h.IsActive);
            }

            return new UserProfileResponse
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CitizenId = user.CitizenId,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Address = user.Address,
                AvatarUrl = user.AvatarUrl,
                Role = role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                CurrentResidence = currentResidence,
                OwnedHousesCount = ownedHousesCount
            };
        }

        public async Task UpdateCurrentUserProfileAsync(string userId, UpdateProfileRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
                throw new NotFoundException("Không tìm thấy thông tin tài khoản người dùng.");

            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new BadRequestException("Họ và tên không được để trống.");

            if (string.IsNullOrWhiteSpace(request.CitizenId))
                throw new BadRequestException("Số CCCD/CMND không được để trống.");

            // Kiểm tra nếu đổi CCCD thì có trùng với ai khác không
            if (!user.CitizenId.Equals(request.CitizenId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                bool citizenExists = await _context.Users.AnyAsync(u => u.Id != userId && u.CitizenId == request.CitizenId.Trim() && u.IsActive);
                if (citizenExists)
                    throw new ConflictException($"Số CCCD '{request.CitizenId}' đã được đăng ký bởi tài khoản khác.");
            }

            // Kiểm tra nếu đổi Email thì có trùng không
            if (!string.IsNullOrWhiteSpace(request.Email) && !string.Equals(user.Email, request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                bool emailExists = await _context.Users.AnyAsync(u => u.Id != userId && u.Email == request.Email.Trim());
                if (emailExists)
                    throw new ConflictException($"Email '{request.Email}' đã được sử dụng bởi tài khoản khác.");

                user.Email = request.Email.Trim();
            }

            // Cập nhật thông tin profile của Domain Entity
            user.UpdateProfile(
                fullName: request.FullName,
                citizenId: request.CitizenId,
                phoneNumber: request.PhoneNumber,
                dateOfBirth: request.DateOfBirth,
                address: request.Address,
                gender: request.Gender,
                avatarUrl: request.AvatarUrl
            );

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                string errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"Cập nhật thông tin thất bại: {errors}");
            }
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                throw new BadRequestException("Mật khẩu hiện tại không được để trống.");

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new BadRequestException("Mật khẩu mới không được để trống.");

            if (request.NewPassword != request.ConfirmNewPassword)
                throw new BadRequestException("Mật khẩu xác nhận không khớp.");

            if (request.NewPassword.Length < 6)
                throw new BadRequestException("Mật khẩu mới phải có ít nhất 6 ký tự.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
                throw new NotFoundException("Không tìm thấy thông tin tài khoản người dùng.");

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                string errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"Đổi mật khẩu thất bại: {errors}");
            }

            // Đổi mật khẩu thành công -> cập nhật Security Stamp để vô hiệu các phiên cũ
            await _userManager.UpdateSecurityStampAsync(user);
        }

        #endregion

        #region 2. QUẢN LÝ NGƯỜI DÙNG DÀNH CHO ADMIN

        public async Task<PagedResult<AdminUserListItemResponse>> GetUsersAsync(UserFilterRequest filter)
        {
            int page = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var query = _context.Users.AsNoTracking();

            // 1. Lọc theo từ khóa (tên, sđt, cccd, email, username)
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                string kw = filter.Keyword.Trim().ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(kw) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(kw)) ||
                    u.CitizenId.Contains(kw) ||
                    (u.Email != null && u.Email.ToLower().Contains(kw)) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(kw))
                );
            }

            // 2. Lọc theo trạng thái hoạt động
            if (filter.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            }

            // 3. Lọc theo dãy trọ (nếu là khách thuê đang ở dãy này)
            if (filter.HouseId.HasValue)
            {
                query = query.Where(u => _context.Contracts.Any(c => 
                    c.AccountId == u.Id && 
                    c.Status == ContractStatus.Active && 
                    c.Room != null && 
                    c.Room.HouseId == filter.HouseId.Value));
            }

            // 4. Lọc theo Role
            if (!string.IsNullOrWhiteSpace(filter.Role))
            {
                var role = await _roleManager.FindByNameAsync(filter.Role.Trim());
                if (role != null)
                {
                    query = query.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == role.Id));
                }
            }

            int totalCount = await query.CountAsync();

            var pagedUsers = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userIds = pagedUsers.Select(u => u.Id).ToList();

            // Lấy thông tin Role của các user đã phân trang (tránh N+1 query)
            var userRoles = await (from ur in _context.UserRoles
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   where userIds.Contains(ur.UserId)
                                   select new { ur.UserId, RoleName = r.Name })
                                  .ToListAsync();

            var rolesDict = userRoles
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.RoleName).FirstOrDefault() ?? "Tenant");

            // Lấy thông tin nơi ở hiện tại (phòng nào, trọ nào, ngày vào) cho các user
            var activeContracts = await _context.Contracts
                .Include(c => c.Room)
                    .ThenInclude(r => r!.House)
                .Where(c => userIds.Contains(c.AccountId) && c.Status == ContractStatus.Active)
                .ToListAsync();

            var residencyDict = activeContracts
                .GroupBy(c => c.AccountId)
                .ToDictionary(g => g.Key, g =>
                {
                    var c = g.First();
                    return new ResidencyInfoDto
                    {
                        RoomId = c.RoomId,
                        RoomNumber = c.Room?.RoomNumber ?? string.Empty,
                        HouseId = c.Room?.HouseId ?? 0,
                        HouseName = c.Room?.House?.HouseName ?? string.Empty,
                        HouseAddress = c.Room?.House?.Address ?? string.Empty,
                        CheckInDate = c.StartDate,
                        ContractEndDate = c.EndDate,
                        RentalPrice = c.RentalPrice,
                        DepositAmount = c.DepositAmount,
                        ContractNumber = c.ContractNumber
                    };
                });

            var resultItems = pagedUsers.Select(u =>
            {
                rolesDict.TryGetValue(u.Id, out var roleName);
                residencyDict.TryGetValue(u.Id, out var residence);
                bool isLockedPermanently = !u.IsActive || (u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow.AddYears(1));

                return new AdminUserListItemResponse
                {
                    UserId = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    CitizenId = u.CitizenId,
                    Email = u.Email,
                    Role = roleName ?? "Tenant",
                    IsActive = u.IsActive,
                    IsLockedPermanently = isLockedPermanently,
                    CreatedAt = u.CreatedAt,
                    CurrentResidence = residence
                };
            }).ToList();

            return new PagedResult<AdminUserListItemResponse>(resultItems, totalCount, page, pageSize);
        }

        public async Task<AdminUserDetailResponse> GetUserDetailAsync(string targetUserId)
        {
            var user = await _userManager.FindByIdAsync(targetUserId);
            if (user == null)
                throw new NotFoundException($"Không tìm thấy tài khoản người dùng với ID '{targetUserId}'.");

            var roles = await _userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault() ?? "Tenant";

            // Lấy lịch sử toàn bộ các hợp đồng thuê của user (cả đang ở và đã thanh lý)
            var allContracts = await _context.Contracts
                .Include(c => c.Room)
                    .ThenInclude(r => r!.House)
                .Where(c => c.AccountId == targetUserId)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            var rentalHistory = allContracts.Select(c => new ResidencyInfoDto
            {
                RoomId = c.RoomId,
                RoomNumber = c.Room?.RoomNumber ?? string.Empty,
                HouseId = c.Room?.HouseId ?? 0,
                HouseName = c.Room?.House?.HouseName ?? string.Empty,
                HouseAddress = c.Room?.House?.Address ?? string.Empty,
                CheckInDate = c.StartDate,
                ContractEndDate = c.EndDate,
                RentalPrice = c.RentalPrice,
                DepositAmount = c.DepositAmount,
                ContractNumber = c.ContractNumber
            }).ToList();

            var activeContract = allContracts.FirstOrDefault(c => c.Status == ContractStatus.Active);
            ResidencyInfoDto? currentResidence = null;
            if (activeContract != null)
            {
                currentResidence = rentalHistory.First(r => r.ContractNumber == activeContract.ContractNumber);
            }

            int? ownedHouses = null;
            if (role == "Landlord" || role == "Admin")
            {
                ownedHouses = await _context.Houses.CountAsync(h => h.OwnerId == targetUserId && h.IsActive);
            }

            bool isLocked = !user.IsActive || (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow.AddYears(1));

            return new AdminUserDetailResponse
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CitizenId = user.CitizenId,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Address = user.Address,
                AvatarUrl = user.AvatarUrl,
                Role = role,
                IsActive = user.IsActive,
                IsLockedPermanently = isLocked,
                LockoutEnd = user.LockoutEnd,
                CreatedAt = user.CreatedAt,
                CurrentResidence = currentResidence,
                OwnedHousesCount = ownedHouses,
                RentalHistory = rentalHistory
            };
        }

        public async Task LockUserPermanentlyAsync(string targetUserId, string currentAdminId, string? reason = null)
        {
            if (targetUserId == currentAdminId)
                throw new BadRequestException("Bạn không thể tự khóa tài khoản Admin của chính mình.");

            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            if (targetUser == null)
                throw new NotFoundException($"Không tìm thấy tài khoản người dùng với ID '{targetUserId}'.");

            // Không cho phép khóa tài khoản Admin khác
            bool isTargetAdmin = await _userManager.IsInRoleAsync(targetUser, "Admin");
            if (isTargetAdmin)
                throw new BadRequestException("Không thể khóa tài khoản có vai trò Quản trị viên (Admin).");

            // 1. Đánh dấu vô hiệu hóa trong Domain
            targetUser.Deactivate();

            // 2. Thiết lập Lockout vĩnh viễn trong ASP.NET Core Identity
            await _userManager.SetLockoutEnabledAsync(targetUser, true);
            await _userManager.SetLockoutEndDateAsync(targetUser, DateTimeOffset.MaxValue);

            // 3. Vô hiệu hóa toàn bộ Refresh Tokens của tài khoản này ngay lập tức (Kick out tức thì)
            var activeTokens = await _context.RefreshTokens
                .Where(r => r.AccountId == targetUserId && !r.IsRevoked)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            // 4. Cập nhật Security Stamp để hủy tính hợp lệ của JWT Token cũ
            await _userManager.UpdateSecurityStampAsync(targetUser);

            await _context.SaveChangesAsync();
        }

        public async Task UnlockUserAsync(string targetUserId, string currentAdminId)
        {
            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            if (targetUser == null)
                throw new NotFoundException($"Không tìm thấy tài khoản người dùng với ID '{targetUserId}'.");

            // 1. Kích hoạt lại trạng thái hoạt động trong Domain
            targetUser.Activate();

            // 2. Mở khóa trong Identity
            await _userManager.SetLockoutEndDateAsync(targetUser, null);
            await _userManager.ResetAccessFailedCountAsync(targetUser);

            await _userManager.UpdateSecurityStampAsync(targetUser);
            await _context.SaveChangesAsync();
        }

        public async Task ResetUserPasswordAsync(string targetUserId, AdminResetPasswordRequest request, string currentAdminId)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new BadRequestException("Mật khẩu mới không được để trống.");

            if (request.NewPassword.Length < 6)
                throw new BadRequestException("Mật khẩu mới phải có ít nhất 6 ký tự.");

            if (request.NewPassword != request.ConfirmNewPassword)
                throw new BadRequestException("Mật khẩu xác nhận không khớp.");

            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            if (targetUser == null)
                throw new NotFoundException($"Không tìm thấy tài khoản người dùng với ID '{targetUserId}'.");

            // 1. Tạo Token reset mật khẩu qua UserManager
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(targetUser);

            // 2. Đặt lại mật khẩu mới
            var result = await _userManager.ResetPasswordAsync(targetUser, resetToken, request.NewPassword);
            if (!result.Succeeded)
            {
                string errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"Cấp lại mật khẩu thất bại: {errors}");
            }

            // 3. Vô hiệu hóa các Refresh Token cũ của user để bắt buộc đăng nhập lại bằng mật khẩu mới
            var activeTokens = await _context.RefreshTokens
                .Where(r => r.AccountId == targetUserId && !r.IsRevoked)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _userManager.UpdateSecurityStampAsync(targetUser);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
