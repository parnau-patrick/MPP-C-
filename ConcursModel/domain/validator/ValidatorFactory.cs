using System;

namespace ConcursModel.domain.validator
{
    public class ValidatorFactory
    {
        public static IValidator<T> CreateValidator<T>() 
        {
            Type type = typeof(T);
            
            if (type == typeof(Event))
                return (IValidator<T>)new EventValidator();
            else if (type == typeof(Participant))
                return (IValidator<T>)new ParticipantValidator();
            else if (type == typeof(User))
                return (IValidator<T>)new UserValidator();
            else
                throw new ArgumentException($"No validator available for type {type.Name}");
        }
    }
}