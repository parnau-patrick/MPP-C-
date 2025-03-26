namespace Laborator3.domain.validator;

public class ValidatorFactory
{
    private static ValidatorFactory _instance;
    
    private ValidatorFactory() { }

    public static ValidatorFactory GetInstance()
    {
        return _instance ??= new ValidatorFactory();
    }

    public IValidator<T> CreateValidator<T>(ValidatorStrategy strategy)
    {
        return strategy switch
        {
            ValidatorStrategy.Event => new EventValidator() as IValidator<T>,
            ValidatorStrategy.Participant => new ParticipantValidator() as IValidator<T>,
            _ => throw new NotImplementedException()
        };
    }
}