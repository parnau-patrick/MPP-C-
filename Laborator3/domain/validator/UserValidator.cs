using System;
using Laborator3.exception;

namespace Laborator3.domain.validator
{
    public class UserValidator : IValidator<User>
    {
        public void Validate(User entity)
        {
            string errors = "";
            
            if (entity == null)
                throw new ValidationException("User cannot be null");
                
            if (string.IsNullOrEmpty(entity.Username))
                errors += "Username cannot be empty\n";
                
            if (string.IsNullOrEmpty(entity.Password))
                errors += "Password cannot be empty\n";
                
            if (string.IsNullOrEmpty(entity.OfficeName))
                errors += "Office name cannot be empty\n";
                
            if (!string.IsNullOrEmpty(errors))
                throw new ValidationException(errors);
        }
    }
}