using System.Net;
using System.Text.Json;
using API.Middlewares;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace UnitTests.Middlewares
{
    public class ExceptionMiddlewareTests
    {
        private readonly Mock<ILogger<ExceptionMiddleware>> _mockLogger;
        private readonly Mock<IHostEnvironment> _mockEnv;

        public ExceptionMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<ExceptionMiddleware>>();
            _mockEnv = new Mock<IHostEnvironment>();
            _mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        }

        [Fact]
        public async Task InvokeAsync_WhenNoExceptionThrown_ShouldCallNextMiddleware()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new ExceptionMiddleware(next, _mockLogger.Object, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.True(nextCalled);
            Assert.Equal((int)HttpStatusCode.OK, context.Response.StatusCode);
        }

        [Theory]
        [InlineData(typeof(ValidationException), HttpStatusCode.BadRequest, "Dữ liệu không hợp lệ")]
        [InlineData(typeof(BadRequestException), HttpStatusCode.BadRequest, "Yêu cầu không hợp lệ")]
        [InlineData(typeof(NotFoundException), HttpStatusCode.NotFound, "Không tìm thấy dữ liệu")]
        [InlineData(typeof(KeyNotFoundException), HttpStatusCode.NotFound, "Khóa không tồn tại")]
        [InlineData(typeof(ConflictException), HttpStatusCode.Conflict, "Dữ liệu bị trùng lặp")]
        [InlineData(typeof(ForbiddenException), HttpStatusCode.Forbidden, "Không có quyền truy cập")]
        [InlineData(typeof(UnauthorizedException), HttpStatusCode.Unauthorized, "Chưa xác thực")]
        [InlineData(typeof(UnauthorizedAccessException), HttpStatusCode.Unauthorized, "Chưa xác thực danh tính")]
        [InlineData(typeof(SecurityTokenException), HttpStatusCode.Unauthorized, "Token lỗi")]
        public async Task InvokeAsync_WhenKnownExceptionThrown_ShouldReturnCorrectStatusCodeAndJson(
            Type exceptionType,
            HttpStatusCode expectedStatusCode,
            string errorMessage)
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var exception = (Exception)Activator.CreateInstance(exceptionType, errorMessage)!;
            RequestDelegate next = (ctx) => throw exception;

            var middleware = new ExceptionMiddleware(next, _mockLogger.Object, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)expectedStatusCode, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            Assert.Equal((int)expectedStatusCode, root.GetProperty("statusCode").GetInt32());
            Assert.Equal(errorMessage, root.GetProperty("message").GetString());
            Assert.True(root.TryGetProperty("timestamp", out _));
            Assert.True(root.TryGetProperty("title", out _));
        }

        [Fact]
        public async Task InvokeAsync_WhenValidationExceptionWithFieldErrors_ShouldIncludeErrorsObject()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var errors = new Dictionary<string, string[]>
            {
                { "PhoneNumber", new[] { "Số điện thoại không hợp lệ." } },
                { "CitizenId", new[] { "Số CCCD phải có 12 chữ số." } }
            };
            var validationException = new ValidationException(errors);

            RequestDelegate next = (ctx) => throw validationException;
            var middleware = new ExceptionMiddleware(next, _mockLogger.Object, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("errors", out var errorsProp));
            Assert.True(errorsProp.TryGetProperty("PhoneNumber", out _));
            Assert.True(errorsProp.TryGetProperty("CitizenId", out _));
        }

        [Fact]
        public async Task InvokeAsync_InProductionMode_ShouldHideInternalServerErrorDetails()
        {
            // Arrange
            var prodEnv = new Mock<IHostEnvironment>();
            prodEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = (ctx) => throw new InvalidOperationException("Internal secret database error");
            var middleware = new ExceptionMiddleware(next, _mockLogger.Object, prodEnv.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            Assert.Equal("Đã xảy ra lỗi không mong muốn từ hệ thống. Vui lòng liên hệ quản trị viên.", root.GetProperty("message").GetString());
            Assert.False(root.TryGetProperty("details", out _));
        }

        [Fact]
        public async Task InvokeAsync_InDevelopmentMode_ShouldIncludeInternalExceptionMessage()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var errorMsg = "Detailed dev error message";
            RequestDelegate next = (ctx) => throw new InvalidOperationException(errorMsg);
            var middleware = new ExceptionMiddleware(next, _mockLogger.Object, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            Assert.Equal(errorMsg, root.GetProperty("message").GetString());
        }

        [Fact]
        public async Task InvokeAsync_WhenDbUpdateConcurrencyException_ShouldReturnConflict()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = (ctx) => throw new DbUpdateConcurrencyException("Concurrency conflict");
            var middleware = new ExceptionMiddleware(next, _mockLogger.Object, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.Conflict, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_WhenSecurityTokenExpiredException_ShouldReturnUnauthorized()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = (ctx) => throw new SecurityTokenExpiredException("Token expired");
            var middleware = new ExceptionMiddleware(next, _mockLogger.Object, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        }
    }
}
