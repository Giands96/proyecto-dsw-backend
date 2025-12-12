using FluentValidation;

namespace Application.Features.Pasajes.Commands;

public class ComprarPasajesCommandValidator : AbstractValidator<ComprarPasajesCommand>
{
    public ComprarPasajesCommandValidator()
    {
        RuleFor(x => x.ViajeId).NotEmpty();
        RuleFor(x => x.UsuarioId).NotEmpty();
        RuleFor(x => x.Pasajeros).NotNull().NotEmpty().WithMessage("Debe indicar al menos un pasajero.");
        RuleForEach(x => x.Pasajeros).ChildRules(p =>
        {
            p.RuleFor(x => x.Nombre).NotEmpty().MaximumLength(120);
        });
        RuleFor(x => x.Pasajeros.Count).LessThanOrEqualTo(50).WithMessage("Máximo 50 pasajes por viaje.");
    }
}
