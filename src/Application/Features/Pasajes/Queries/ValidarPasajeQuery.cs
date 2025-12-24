using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Pasajes.Queries;

public record ValidarPasajeQuery(string QrContent) : IRequest<ValidarPasajeResult>;

public record ValidarPasajeResult(
    bool IsValid,
    Guid? PasajeId,
    Guid? ViajeId,
    string? NombrePasajero,
    string? Destino,
    DateTimeOffset? FechaHora
);

public class ValidarPasajeQueryHandler
    : IRequestHandler<ValidarPasajeQuery, ValidarPasajeResult>
{
    private readonly IAppDbContext _context;

    public ValidarPasajeQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ValidarPasajeResult> Handle(
        ValidarPasajeQuery request,
        CancellationToken cancellationToken
    )
    {
        var pasajeIdRaw = ExtractPasajeId(request.QrContent);

        if (!Guid.TryParse(pasajeIdRaw, out var pasajeId))
            return Invalid();

        var pasaje = await _context.Pasajes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pasajeId, cancellationToken);

        if (pasaje is null)
            return Invalid();

        var viaje = await _context.Viajes
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == pasaje.ViajeId, cancellationToken);

        return new ValidarPasajeResult(
            IsValid: true,
            PasajeId: pasaje.Id,
            ViajeId: pasaje.ViajeId,
            NombrePasajero: pasaje.NombrePasajero,
            Destino: viaje?.Destino,
            FechaHora: viaje?.FechaHora
        );

    }

    private static ValidarPasajeResult Invalid() =>
        new(false, null, null, null, null, null);

    private static string? ExtractPasajeId(string? qrContent)
    {
        if (string.IsNullOrWhiteSpace(qrContent))
            return null;

        if (Guid.TryParse(qrContent, out _))
            return qrContent;

        if (Uri.TryCreate(qrContent, UriKind.Absolute, out var uri))
        {
            var segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            return segments.LastOrDefault();
        }

        var lastSlash = qrContent.LastIndexOf('/');
        return lastSlash >= 0
            ? qrContent[(lastSlash + 1)..]
            : qrContent;
    }
}
