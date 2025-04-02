using System;

namespace Laborator3.exception
{
    public class RepositoryException : Exception
    {
        public RepositoryException(string message) : base(message) { }
        public RepositoryException(string message, Exception innerException) : base(message, innerException) { }
    }
}