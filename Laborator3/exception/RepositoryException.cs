namespace SwimmingCompetition.exception;

public class RepositoryException : Exception
{
    public RepositoryException() { }
    
    public RepositoryException(string message) : base(message) { }
    
    public RepositoryException(string message, Exception cause) : base(message, cause) { }
}