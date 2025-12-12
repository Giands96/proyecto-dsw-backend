using Application.Common.Interfaces;
using Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Viajes.Queries;

public record GetViajeByIdQuery(Guid Id) : IRequest<ViajeDto?>;

public class GetViajeByIdQueryHandler : IRequestHandler<GetViajeByIdQuery, ViajeDto?>
{
    private readonly IAppDbContext _context;

    public GetViajeByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ViajeDto?> Handle(GetViajeByIdQuery request, CancellationToken cancellationToken)
    {
        var viaje = await _context.Viajes.FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);
        if (viaje is null)
            return null;

        var vendidos = await _context.Pasajes.CountAsync(p => p.ViajeId == viaje.Id, cancellationToken);
        return new ViajeDto(viaje.Id, viaje.Destino, viaje.FechaHora, viaje.CostoBase, viaje.AsientosDisponibles(vendidos));
    }
}
