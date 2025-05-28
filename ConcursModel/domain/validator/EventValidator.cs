using System;
using ConcursModel.exception;

namespace ConcursModel.domain.validator
{
    public class EventValidator : IValidator<Event>
    {
        public void Validate(Event entity)
        {
            string errors = "";
            
            if (entity == null)
                throw new ValidationException("Event cannot be null");
                
            if (string.IsNullOrEmpty(entity.Distance))
                errors += "Event distance cannot be empty\n";
                
            if (string.IsNullOrEmpty(entity.Style))
                errors += "Event style cannot be empty\n";
                
            if (!string.IsNullOrEmpty(errors))
                throw new ValidationException(errors);
        }
    }
}