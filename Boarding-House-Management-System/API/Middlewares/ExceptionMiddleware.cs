using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning(ex, "Response headers đã được gửi về client, không thể can thiệp xử lý ngoại lệ.");
                    throw;
                }

                _logger.LogError(
                    ex,
                    "Lỗi ngoại lệ chưa được xử lý tại HTTP {Method} {Path}: {Message}",
                    context.Request.Method,
                    context.Request.Path,
                    ex.Message);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, title, message, errors) = exception switch
            {
                ValidationException valEx => (
                    HttpStatusCode.BadRequest,
                    "Dữ liệu không hợp lệ",
                    valEx.Message,
                    (object?)valEx.Errors
                ),
                BadRequestException badReqEx => (
                    HttpStatusCode.BadRequest,
                    "Yêu cầu không hợp lệ",
                    badReqEx.Message,
                    null
                ),
                NotFoundException notFoundEx => (
                    HttpStatusCode.NotFound,
                    "Không tìm thấy tài nguyên",
                    notFoundEx.Message,
                    null
                ),
                KeyNotFoundException keyNotFoundEx => (
                    HttpStatusCode.NotFound,
                    "Không tìm thấy tài nguyên",
                    keyNotFoundEx.Message,
                    null
                ),
                ConflictException conflictEx => (
                    HttpStatusCode.Conflict,
                    "Xung đột dữ liệu",
                    conflictEx.Message,
                    null
                ),
                ForbiddenException forbiddenEx => (
                    HttpStatusCode.Forbidden,
                    "Không có quyền truy cập",
                    forbiddenEx.Message,
                    null
                ),
                UnauthorizedException unauthCustomEx => (
                    HttpStatusCode.Unauthorized,
                    "Chưa xác thực",
                    unauthCustomEx.Message,
                    null
                ),
                UnauthorizedAccessException unauthEx => (
                    HttpStatusCode.Unauthorized,
                    "Chưa xác thực",
                    unauthEx.Message,
                    null
                ),
                SecurityTokenExpiredException => (
                    HttpStatusCode.Unauthorized,
                    "Phiên làm việc hết hạn",
                    "Token xác thực đã hết hạn. Vui lòng làm mới token hoặc đăng nhập lại.",
                    null
                ),
                SecurityTokenException tokenEx => (
                    HttpStatusCode.Unauthorized,
                    "Token không hợp lệ",
                    tokenEx.Message,
                    null
                ),
                DbUpdateConcurrencyException => (
                    HttpStatusCode.Conflict,
                    "Xung đột dữ liệu đồng thời",
                    "Dữ liệu đã bị thay đổi bởi người dùng khác. Vui lòng tải lại và thử lại.",
                    null
                ),
                DbUpdateException dbEx => (
                    HttpStatusCode.BadRequest,
                    "Lỗi cơ sở dữ liệu",
                    _env.IsDevelopment()
                        ? dbEx.InnerException?.Message ?? dbEx.Message
                        : "Không thể lưu dữ liệu do vi phạm ràng buộc toàn vẹn.",
                    null
                ),
                _ => (
                    HttpStatusCode.InternalServerError,
                    "Lỗi hệ thống",
                    _env.IsDevelopment()
                        ? exception.Message
                        : "Đã xảy ra lỗi không mong muốn từ hệ thống. Vui lòng liên hệ quản trị viên.",
                    null
                )
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponse
            {
                StatusCode = (int)statusCode,
                Title = title,
                Message = message,
                Errors = errors,
                Path = context.Request.Path.Value,
                Timestamp = DateTime.UtcNow,
                Details = _env.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError
                    ? exception.StackTrace
                    : null
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Errors { get; set; }
        public string? Path { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
