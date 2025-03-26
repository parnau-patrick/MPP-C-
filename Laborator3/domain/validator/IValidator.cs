namespace Laborator3.domain.validator
{
    public interface IValidator<T>
    {
        bool Validate(T entity);
    }
}