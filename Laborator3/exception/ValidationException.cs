using System;

namespace Laborator3.exception
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}