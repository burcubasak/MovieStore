using FluentValidation;
using MovieStore.MovieStore.Schema;
using System;

namespace MovieStore.MovieStore.API.Cqrs.Validations 
{
    public class DirectorValidator : AbstractValidator<DirectorRequest>
    {
        public DirectorValidator()
        {
            RuleFor(director => director.Name)
                .NotEmpty().WithMessage("Director's name cannot be empty.")
                .MinimumLength(2).WithMessage("Director's name must be at least 2 characters long.")
                .MaximumLength(50).WithMessage("Director's name cannot exceed 50 characters.");

            RuleFor(director => director.SurName)
                .NotEmpty().WithMessage("Director's surname cannot be empty.")
                .MinimumLength(2).WithMessage("Director's surname must be at least 2 characters long.")
                .MaximumLength(50).WithMessage("Director's surname cannot exceed 50 characters.");

            RuleFor(director => director.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth cannot be empty.")
                .LessThan(DateTime.Now.AddYears(-10)).WithMessage("Director must be at least 10 years old.")
                .GreaterThan(DateTime.Now.AddYears(-100)).WithMessage("Director cannot be older than 100 years.");
        }
    }
}
