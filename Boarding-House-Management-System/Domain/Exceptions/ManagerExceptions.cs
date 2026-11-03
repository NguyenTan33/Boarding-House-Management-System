namespace Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }

    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }

    public class ValidationException : Exception
    {
        public IDictionary<string, string[]>? Errors { get; }

        public ValidationException(string message) : base(message) { }

        public ValidationException(string message, IDictionary<string, string[]> errors) : base(message)
        {
            Errors = errors;
        }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("Một hoặc nhiều lỗi xác thực dữ liệu đã xảy ra.")
        {
            Errors = errors;
        }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "Chưa xác thực danh tính người dùng.") : base(message) { }
    }
}
