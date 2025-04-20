using FluentValidation;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.Validations
{
    public class ActorValidator:AbstractValidator<ActorRequest>
    {
        public ActorValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .Length(2, 50)
                .WithMessage("Name must be between 2 and 50 characters long.");
            RuleFor(x => x.SurName)
                .NotEmpty()
                .WithMessage("Surname is required.")
                .Length(2, 50)
                .WithMessage("Surname must be between 2 and 50 characters long.");
            
        }
    }
 
}
