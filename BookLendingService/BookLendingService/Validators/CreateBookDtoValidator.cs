using BookLending.Application.DTOs;
using FluentValidation;

namespace BookLending.Validators;

public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(500).WithMessage("Title cannot exceed 500 characters.");

        RuleFor(x => x.Author)
            .MaximumLength(200).WithMessage("Author name cannot exceed 200 characters.");
    }
}
