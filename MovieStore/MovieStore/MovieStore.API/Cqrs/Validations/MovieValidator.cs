using FluentValidation;
using MovieStore.MovieStore.Schema;

namespace MovieStore.MovieStore.API.Cqrs.Validations
{
    public class MovieValidator:AbstractValidator<MovieRequest>
    {
        public MovieValidator()
        {
            RuleFor(m => m.Title)
                .NotEmpty().WithMessage("Title is required.")
                .Length(1, 100).WithMessage("Title must be between 1 and 100 characters.");

            RuleFor(m => m.Year)
                .NotEmpty().WithMessage("Year is required.")
                .InclusiveBetween(1888, DateTime.Now.Year).WithMessage($"Year must be between 1888 and {DateTime.Now.Year}.");
            RuleFor(m => m.Genre)
                .NotEmpty().WithMessage("Genre is required.")
                .Length(1, 50).WithMessage("Genre must be between 1 and 50 characters.");
            RuleFor(m => m.Price)
                .NotEmpty().WithMessage("Price is required.")
                .GreaterThan(0).WithMessage("Price must be greater than 0.");
        }
    }

}
