using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Pasajes.Queries;

public record ValidarPasajeQuery(string QrContent) : IRequest<ValidarPasajeResult>;

public record ValidarPasajeResult(bool IsValid, Guid? PasajeId, Guid? ViajeId, string? NombrePasajero, string? Destino, DateTimeOffset? FechaHora);

public class ValidarPasajeQueryHandler : IRequestHandler<ValidarPasajeQuery, ValidarPasajeResult>
{
    private readonly IAppDbContext _context;

    public ValidarPasajeQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ValidarPasajeResult> Handle(ValidarPasajeQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.QrContent, out var pasajeId))
            return new ValidarPasajeResult(false, null, null, null, null, null);

        var pasaje = await _context.Pasajes.FirstOrDefaultAsync(p => p.Id == pasajeId, cancellationToken);
        if (pasaje is null)
            return new ValidarPasajeResult(false, null, null, null, null, null);

        var viaje = await _context.Viajes.FirstOrDefaultAsync(v => v.Id == pasaje.ViajeId, cancellationToken);
        return new ValidarPasajeResult(true, pasaje.Id, pasaje.ViajeId, pasaje.NombrePasajero, viaje?.Destino, viaje?.FechaHora);
    }
}
