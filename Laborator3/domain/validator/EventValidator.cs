using System.ComponentModel.DataAnnotations;

namespace Laborator3.domain.validator;

public class EventValidator: IValidator<Event>
{
    public bool Validate(Event entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), "Event cannot be null");
        }
        return true; 
    }
    

}