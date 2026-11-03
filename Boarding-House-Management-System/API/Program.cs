using System.Threading.RateLimiting;
using API.Middlewares;
using Application;
using Infrastructure;
using Infrastructure.Services.Auth;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký tầng Infrastructure (DbContext, SQL Server, Identity, JWT, Repositories, Auth Services)
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Đăng ký tầng Application (Business Services, Strategy Pattern, Anomaly Detection)
builder.Services.AddApplicationServices();

// 3. Cấu hình Rate Limiting bảo vệ hệ thống và từng Endpoint chuyên biệt
builder.Services.AddRateLimiter(options =>
{
    // Thiết lập HTTP status code 429 Too Many Requests kèm body JSON thông báo chi tiết
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        var errorResponse = new
        {
            statusCode = 429,
            title = "Quá nhiều yêu cầu (Rate Limit Exceeded)",
            message = "Bạn đã gửi quá nhiều yêu cầu trong thời gian ngắn. Vui lòng thử lại sau.",
            retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) 
                ? (int)retryAfter.TotalSeconds 
                : 60
        };
        await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken: token);
    };

    // 3.1. AuthPolicy: Giới hạn nghiêm ngặt chống Brute-Force mật khẩu (10 request / 1 phút)
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    // 3.2. StandardPolicy: Thao tác CRUD thông thường cho phòng, hợp đồng, khách thuê (60 request / 1 phút)
    options.AddSlidingWindowLimiter("StandardPolicy", opt =>
    {
        opt.PermitLimit = 60;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 4;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

    // 3.3. HeavyPolicy: Các tác vụ tính toán nặng (Sinh hóa đơn hàng loạt, ghi số điện nước bulk, thống kê) (15 request / 1 phút)
    options.AddFixedWindowLimiter("HeavyPolicy", opt =>
    {
        opt.PermitLimit = 15;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    // 3.4. WebhookPolicy: Dành riêng cho cổng thanh toán Webhook SePay/Ngân hàng (120 request / 1 phút)
    options.AddFixedWindowLimiter("WebhookPolicy", opt =>
    {
        opt.PermitLimit = 120;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// 4. Tự động Seed Roles (Admin, Landlord, Tenant) và tài khoản Admin mặc định
await IdentityDbSeeder.SeedRolesAndAdminAsync(app.Services);

// 5. Middleware xử lý ngoại lệ tập trung (trả về JSON đẹp, mã lỗi chuẩn)
app.UseGlobalExceptionMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 6. Áp dụng Rate Limiting vào pipeline
app.UseRateLimiter();

// 7. Bật Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
