using FluentValidation;

namespace Application.Features.Auth;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(120);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
