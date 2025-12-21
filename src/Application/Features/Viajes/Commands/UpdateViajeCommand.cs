using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Viajes.Commands;

public record UpdateViajeCommand(Guid Id, string Destino, DateTimeOffset FechaHora, decimal CostoBase, int CapacidadMax) : IRequest<bool>;

public class UpdateViajeHandler : IRequestHandler<UpdateViajeCommand, bool>
{
    private readonly IAppDbContext _context;

    public UpdateViajeHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(UpdateViajeCommand request, CancellationToken token)
    {
        var entity = await _context.Viajes.FindAsync(new object[] { request.Id }, token);
        if (entity == null) return false; 
        entity.Destino = request.Destino;
        entity.FechaHora = request.FechaHora;
        entity.CostoBase = request.CostoBase;
        entity.CapacidadMax = request.CapacidadMax;

        await _context.SaveChangesAsync(token);
        return true;
    }
}