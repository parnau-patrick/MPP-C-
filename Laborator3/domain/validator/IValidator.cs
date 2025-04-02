using System;

namespace Laborator3.domain.validator
{
    public interface IValidator<T>
    {
        void Validate(T entity);
    }
}