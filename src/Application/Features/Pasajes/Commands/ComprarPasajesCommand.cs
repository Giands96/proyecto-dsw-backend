using Application.Common.Interfaces;
using Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Pasajes.Commands;

public record PasajeroInput(string Nombre);

public record ComprarPasajesCommand(Guid ViajeId, List<PasajeroInput> Pasajeros, Guid UsuarioId) : IRequest<IReadOnlyList<PasajeCardDto>>;

public class ComprarPasajesCommandHandler : IRequestHandler<ComprarPasajesCommand, IReadOnlyList<PasajeCardDto>>
{
    private readonly IAppDbContext _context;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IQRGenerator _qrGenerator;

    public ComprarPasajesCommandHandler(IAppDbContext context, IPaymentGateway paymentGateway, IQRGenerator qrGenerator)
    {
        _context = context;
        _paymentGateway = paymentGateway;
        _qrGenerator = qrGenerator;
    }

    public async Task<IReadOnlyList<PasajeCardDto>> Handle(ComprarPasajesCommand request, CancellationToken cancellationToken)
    {
        var viaje = await _context.Viajes.FirstOrDefaultAsync(v => v.Id == request.ViajeId, cancellationToken);
        if (viaje is null)
            throw new InvalidOperationException("Viaje no encontrado.");

        var cantidadSolicitada = request.Pasajeros.Count;
        var vendidos = await _context.Pasajes.CountAsync(p => p.ViajeId == viaje.Id, cancellationToken);
        if (vendidos + cantidadSolicitada > viaje.CapacidadMax)
            throw new InvalidOperationException("Capacidad excedida.");

        var total = viaje.CostoBase * cantidadSolicitada;
        var payment = await _paymentGateway.ChargeAsync(total, cancellationToken);
        if (!payment.Success)
            throw new InvalidOperationException("Pago rechazado.");

        var pasajes = new List<Domain.Entities.Pasaje>(cantidadSolicitada);
        foreach (var pasajero in request.Pasajeros)
        {
            var pasaje = new Domain.Entities.Pasaje
            {
                Id = Guid.NewGuid(),
                ViajeId = viaje.Id,
                UsuarioCompradorId = request.UsuarioId,
                NombrePasajero = pasajero.Nombre,
                Costo = viaje.CostoBase,
                CreatedAt = DateTime.UtcNow
            };
            pasaje.QRData = _qrGenerator.GenerateQrBase64(pasaje.Id.ToString());
            pasajes.Add(pasaje);
        }

        _context.Pasajes.AddRange(pasajes);
        await _context.SaveChangesAsync(cancellationToken);

        return pasajes
            .Select(p => new PasajeCardDto(p.Id, p.NombrePasajero, p.Costo, p.QRData))
            .ToList();
    }
}
