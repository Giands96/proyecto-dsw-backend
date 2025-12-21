using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Pasajes.Queries;

public record PasajeAdminDto(Guid Id, string NombrePasajero, decimal Costo, string CompradorEmail, DateTimeOffset FechaCompra);

public record GetPasajesByViajeIdQuery(Guid ViajeId) : IRequest<List<PasajeAdminDto>>;

public class GetPasajesByViajeIdHandler : IRequestHandler<GetPasajesByViajeIdQuery, List<PasajeAdminDto>>
{
    private readonly IAppDbContext _context;
    public GetPasajesByViajeIdHandler(IAppDbContext context) => _context = context;

    public async Task<List<PasajeAdminDto>> Handle(GetPasajesByViajeIdQuery request, CancellationToken token)
    {
        return await _context.Pasajes.Where(p => p.ViajeId == request.ViajeId)
            .Select(p => new PasajeAdminDto(
                p.Id,
                p.NombrePasajero,
                p.Costo,
                _context.Usuarios
                    .Where(u => u.Id == p.UsuarioCompradorId)
                    .Select(u => u.Email)
                    .FirstOrDefault() ?? "Usuario Eliminado",
                p.CreatedAt
            ))
            .ToListAsync(token);

    }
}