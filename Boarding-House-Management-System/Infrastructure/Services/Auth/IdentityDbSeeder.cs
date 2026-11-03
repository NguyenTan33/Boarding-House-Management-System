using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Auth
{
    public static class IdentityDbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();

            // 1. Khởi tạo các Vai trò (Roles) trong hệ thống Nhà Trọ
            var roles = new[] { "Admin", "Landlord", "Tenant" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Khởi tạo tài khoản Quản trị viên (Admin) mặc định nếu chưa tồn tại
            var adminPhone = "+84999999999";
            var existingAdmin = await userManager.FindByNameAsync(adminPhone);
            if (existingAdmin == null)
            {
                var adminUser = new Account(
                    userName: adminPhone,
                    email: "admin@boardinghouse.local",
                    fullName: "Quản trị viên Hệ thống",
                    citizenId: "000000000000",
                    phoneNumber: adminPhone
                );

                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(adminUser, new[] { "Admin", "Landlord", "Tenant" });
                }
            }
        }
    }
}
