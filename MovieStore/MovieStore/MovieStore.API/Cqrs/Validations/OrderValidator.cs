using FluentValidation;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.Validations
{ 
public class OrderCreateRequestValidator : AbstractValidator<OrderCreateRequest>
{
    public OrderCreateRequestValidator()
    {
        RuleFor(request => request.MovieId)
            .NotEmpty().WithMessage("Movie ID cannot be empty.")
            .NotEqual(Guid.Empty).WithMessage("Movie ID cannot be an empty GUID.");
    }
}
}