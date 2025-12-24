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
        try
        {
            var pasajeIdRaw = ExtractPasajeId(request.QrContent);
            if (string.IsNullOrWhiteSpace(pasajeIdRaw) || !Guid.TryParse(pasajeIdRaw, out var pasajeId))
                throw new FormatException("El identificador del pasaje es inválido.");

            var pasaje = await _context.Pasajes.FirstOrDefaultAsync(p => p.Id == pasajeId, cancellationToken);
            if (pasaje is null)
                throw new KeyNotFoundException("El pasaje no existe en la base de datos.");

            if (pasaje.Id != pasajeId)
                throw new InvalidOperationException("El identificador del pasaje no coincide con el código escaneado.");

            var viaje = await _context.Viajes.FirstOrDefaultAsync(v => v.Id == pasaje.ViajeId, cancellationToken);
            return new ValidarPasajeResult(true, pasaje.Id, pasaje.ViajeId, pasaje.NombrePasajero, viaje?.Destino, viaje?.FechaHora);
        }
        catch
        {
            return InvalidResult;
        }
    }

    private static ValidarPasajeResult InvalidResult => new(false, null, null, null, null, null);

    private static string? ExtractPasajeId(string? qrContent)
    {
        if (string.IsNullOrWhiteSpace(qrContent))
            return null;

        if (Guid.TryParse(qrContent, out _))
            return qrContent;

        if (Uri.TryCreate(qrContent, UriKind.Absolute, out var uri))
        {
            var trimmedPath = uri.AbsolutePath.Trim('/');
            if (!string.IsNullOrWhiteSpace(trimmedPath))
            {
                var lastSlash = trimmedPath.LastIndexOf('/');
                return lastSlash >= 0 ? trimmedPath[(lastSlash + 1)..] : trimmedPath;
            }
        }

        var fallbackSlash = qrContent.LastIndexOf('/');
        if (fallbackSlash >= 0 && fallbackSlash + 1 < qrContent.Length)
            return qrContent[(fallbackSlash + 1)..];

        return null;
    }
}
