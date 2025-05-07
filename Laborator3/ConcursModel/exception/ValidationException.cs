using System;

namespace ConcursModel.exception
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}