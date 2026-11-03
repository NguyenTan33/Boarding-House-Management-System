namespace Application.Exceptions
{
    public class NotFoundException : Domain.Exceptions.NotFoundException
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class BadRequestException : Domain.Exceptions.BadRequestException
    {
        public BadRequestException(string message) : base(message) { }
    }

    public class ConflictException : Domain.Exceptions.ConflictException
    {
        public ConflictException(string message) : base(message) { }
    }

    public class ValidationException : Domain.Exceptions.ValidationException
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(IDictionary<string, string[]> errors) : base(errors) { }
    }

    public class ForbiddenException : Domain.Exceptions.ForbiddenException
    {
        public ForbiddenException(string message) : base(message) { }
    }
}
