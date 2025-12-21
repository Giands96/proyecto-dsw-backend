using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Viajes.Commands;

public record CreateViajeCommand(string Destino, DateTimeOffset FechaHora, decimal CostoBase, int CapacidadMax) : IRequest<Guid>;

public class CreateViajeHandler : IRequestHandler<CreateViajeCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateViajeHandler(IAppDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateViajeCommand request, CancellationToken token)
    {
        var entity = new Viaje
        {
            Id = Guid.NewGuid(),
            Destino = request.Destino,
            FechaHora = request.FechaHora,
            CostoBase = request.CostoBase,
            CapacidadMax = request.CapacidadMax
        };

        _context.Viajes.Add(entity);
        await _context.SaveChangesAsync(token);

        return entity.Id;
    }
}