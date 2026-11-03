using API.Middlewares;
using Infrastructure;
using Infrastructure.Services.Auth;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký tầng Infrastructure (DbContext, SQL Server, Identity, JWT, Auth Services)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// 2. Tự động Seed Roles (Admin, Landlord, Tenant) và tài khoản Admin mặc định
await IdentityDbSeeder.SeedRolesAndAdminAsync(app.Services);

// 3. Middleware xử lý ngoại lệ tập trung (trả về JSON đẹp, mã lỗi chuẩn)
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 4. Bật Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
