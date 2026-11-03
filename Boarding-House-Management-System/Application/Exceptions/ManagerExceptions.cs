using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message){}
    }


    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message){}
    }


    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message){}
    }


    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message){}
    }


    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message){}
    }

}
