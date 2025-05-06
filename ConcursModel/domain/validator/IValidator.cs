using System;

namespace ConcursModel.domain.validator
{
    public interface IValidator<T>
    {
        void Validate(T entity);
    }
}