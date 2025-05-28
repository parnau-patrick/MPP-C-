using System;
using ConcursModel.exception;

namespace ConcursModel.domain.validator
{
    public class ParticipantValidator : IValidator<Participant>
    {
        public void Validate(Participant entity)
        {
            string errors = "";
            
            if (entity == null)
                throw new ValidationException("Participant cannot be null");
                
            if (string.IsNullOrEmpty(entity.Name))
                errors += "Participant name cannot be empty\n";
                
            if (entity.Age <= 0)
                errors += "Participant age must be positive\n";
                
            if (!string.IsNullOrEmpty(errors))
                throw new ValidationException(errors);
        }
    }
}