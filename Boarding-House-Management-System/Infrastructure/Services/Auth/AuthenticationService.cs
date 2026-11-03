using Application.DTOs.Auth;
using Application.Interfaces.Auth;
using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Auth
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<Account> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IPhoneService _phoneService;
        private readonly ITokenService _tokenService;
        private readonly IHashService _hashService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationService(
            UserManager<Account> userManager,
            RoleManager<IdentityRole> roleManager,
            IPhoneService phoneService,
            ITokenService tokenService,
            IHashService hashService,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _phoneService = phoneService;
            _tokenService = tokenService;
            _hashService = hashService;
            _context = context;
            _configuration = configuration;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            // 1. Kiểm tra tính hợp lệ cơ bản
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ValidationException("Họ và tên không được để trống.");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                throw new ValidationException("Số điện thoại không được để trống.");

            if (string.IsNullOrWhiteSpace(request.CitizenId))
                throw new ValidationException("Số CCCD/CMND không được để trống.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new ValidationException("Mật khẩu phải có độ dài tối thiểu từ 6 ký tự trở lên.");

            // 2. Chuẩn hóa & kiểm tra số điện thoại
            var phoneNumber = _phoneService.Normalize(request.PhoneNumber);
            if (!_phoneService.IsValid(phoneNumber))
                throw new ValidationException("Định dạng số điện thoại không hợp lệ (vd: 0912345678).");

            // 3. Kiểm tra trùng số điện thoại
            var isPhoneTaken = await _userManager.Users.AnyAsync(x => x.PhoneNumber == phoneNumber);
            if (isPhoneTaken)
                throw new ConflictException("Số điện thoại này đã được đăng ký trong hệ thống.");

            // 4. Kiểm tra trùng số CCCD
            var isCitizenIdTaken = await _userManager.Users.AnyAsync(x => x.CitizenId == request.CitizenId.Trim());
            if (isCitizenIdTaken)
                throw new ConflictException("Số CCCD/CMND này đã được sử dụng.");

            // 5. Xác định quyền (Role): Chỉ chấp nhận "Landlord" hoặc "Tenant"
            var role = "Tenant";
            if (!string.IsNullOrWhiteSpace(request.Role) && 
                request.Role.Equals("Landlord", StringComparison.OrdinalIgnoreCase))
            {
                role = "Landlord";
            }

            // Đảm bảo Role đã tồn tại trong database
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            // 6. Tạo thực thể Account
            var email = !string.IsNullOrWhiteSpace(request.Email)
                ? request.Email.Trim()
                : $"{phoneNumber.Replace("+", "")}@boardinghouse.local";

            var account = new Account(
                userName: phoneNumber,
                email: email,
                fullName: request.FullName,
                citizenId: request.CitizenId,
                phoneNumber: phoneNumber
            );

            // 7. Tạo tài khoản trong ASP.NET Core Identity
            var createResult = await _userManager.CreateAsync(account, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new BadRequestException($"Đăng ký không thành công: {errors}");
            }

            // 8. Gán Role
            await _userManager.AddToRoleAsync(account, role);

            return new RegisterResponse
            {
                UserId = account.Id,
                FullName = account.FullName,
                PhoneNumber = account.PhoneNumber ?? phoneNumber,
                Message = $"Đăng ký tài khoản thành công với vai trò {(role == "Landlord" ? "Chủ trọ" : "Khách thuê")}."
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                throw new ValidationException("Số điện thoại không được để trống.");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ValidationException("Mật khẩu không được để trống.");

            var phoneNumber = _phoneService.Normalize(request.PhoneNumber);

            // Tìm tài khoản theo SĐT hoặc UserName
            var account = await _userManager.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber || x.UserName == phoneNumber);

            if (account == null)
                throw new BadRequestException("Số điện thoại hoặc mật khẩu không chính xác.");

            // Kiểm tra trạng thái khóa tài khoản
            if (!account.IsActive)
                throw new ForbiddenException("Tài khoản của bạn đã bị tạm khóa. Vui lòng liên hệ quản trị viên.");

            // Xác thực mật khẩu
            var isPasswordValid = await _userManager.CheckPasswordAsync(account, request.Password);
            if (!isPasswordValid)
                throw new BadRequestException("Số điện thoại hoặc mật khẩu không chính xác.");

            // Sinh Access Token (JWT)
            var accessToken = await _tokenService.GenerateAccessTokenAsync(account);

            // Sinh Refresh Token
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenSecret = _configuration["RefreshToken:Secret"] 
                ?? "DefaultSecretKeyForRefreshTokenHashingSecure2026!";
            var refreshTokenHash = _hashService.Hash(refreshToken, refreshTokenSecret);

            var expireDays = int.Parse(_configuration["RefreshToken:ExpireDays"] ?? "7");

            // Lưu Refresh Token vào Database
            var refreshTokenEntity = new RefreshToken
            {
                AccountId = account.Id,
                TokenHash = refreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(expireDays),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60");
            var roles = (await _userManager.GetRolesAsync(account)).ToList();

            return new LoginResponse
            {
                UserId = account.Id,
                FullName = account.FullName,
                PhoneNumber = account.PhoneNumber ?? phoneNumber,
                Roles = roles,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expireMinutes)
            };
        }

        public async Task LogoutAsync(LogoutRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new ValidationException("Refresh Token không được để trống.");

            var refreshTokenSecret = _configuration["RefreshToken:Secret"] 
                ?? "DefaultSecretKeyForRefreshTokenHashingSecure2026!";

            var refreshTokenHash = _hashService.Hash(request.RefreshToken, refreshTokenSecret);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == refreshTokenHash);

            if (storedToken == null)
                throw new BadRequestException("Refresh Token không hợp lệ.");

            if (storedToken.IsRevoked)
                return; // Đã thu hồi trước đó rồi

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<UserProfileResponse> GetCurrentUserProfileAsync(string userId)
        {
            var account = await _userManager.FindByIdAsync(userId);
            if (account == null)
                throw new NotFoundException("Không tìm thấy thông tin người dùng.");

            var roles = (await _userManager.GetRolesAsync(account)).ToList();

            return new UserProfileResponse
            {
                UserId = account.Id,
                FullName = account.FullName,
                PhoneNumber = account.PhoneNumber ?? string.Empty,
                Email = account.Email,
                CitizenId = account.CitizenId,
                DateOfBirth = account.DateOfBirth,
                Gender = account.Gender,
                Address = account.Address,
                AvatarUrl = account.AvatarUrl,
                Roles = roles
            };
        }
    }
}
