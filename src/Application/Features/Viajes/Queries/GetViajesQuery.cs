using Application.Common.Interfaces;
using Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Viajes.Queries;

public record GetViajesQuery(
    string? Destino,
     DateTimeOffset? Desde,
      DateTimeOffset? Hasta,
       int Page = 1,
        int PageSize = 20
        ) : IRequest<IReadOnlyList<ViajeDto>>;

public class GetViajesQueryHandler : IRequestHandler<GetViajesQuery, IReadOnlyList<ViajeDto>>
{
    private readonly IAppDbContext _context;

    public GetViajesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ViajeDto>> Handle(GetViajesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Viajes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Destino))
            query = query.Where(v => v.Destino.Contains(request.Destino));

        if (request.Desde.HasValue)
            query = query.Where(v => v.FechaHora >= request.Desde.Value);

        if (request.Hasta.HasValue)
            query = query.Where(v => v.FechaHora <= request.Hasta.Value);

        query = query.OrderBy(v => v.FechaHora);

        var skip = (request.Page - 1) * request.PageSize;
        var viajes = await query.Skip(skip).Take(request.PageSize).ToListAsync(cancellationToken);

        var vendidosPorViaje = await _context.Pasajes
            .GroupBy(p => p.ViajeId)
            .Select(g => new { ViajeId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var dict = vendidosPorViaje.ToDictionary(x => x.ViajeId, x => x.Count);

        return viajes.Select(v => new ViajeDto(
            v.Id,
            v.Destino,
            v.FechaHora,
            v.CostoBase,
            v.AsientosDisponibles(dict.GetValueOrDefault(v.Id, 0))
        )).ToList();
    }
}
