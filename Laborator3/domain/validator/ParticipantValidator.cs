namespace Laborator3.domain.validator;

public class ParticipantValidator : IValidator<Participant>
{
        public bool Validate(Participant entity)
        {
                if (entity == null)
                {
                        throw new ArgumentNullException(nameof(entity), "Participant cannot be null");
                }

                return true;
        }
}