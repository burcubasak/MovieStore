using FluentValidation;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.Validations
{
    public class AssignActorToMovieRequestValidator : AbstractValidator<AssignActorToMovieRequest>
    {
        public AssignActorToMovieRequestValidator()
        {
            RuleFor(request => request.ActorId)
                .NotEmpty().WithMessage("Actor ID cannot be empty.");
        }
    }
}
