using System;

namespace InsuranceAgency.Domain.Exceptions
{
    public class ValidationException : DomainException
    {
        public ValidationException() { }
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception inner) : base(message, inner) { }
    }
}
