using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Pasajes.Commands;

public record UpdatePasajeCommand(Guid Id, string NombrePasajero) : IRequest<bool>;

public class UpdatePasajeHandler : IRequestHandler<UpdatePasajeCommand, bool>
{
    private readonly IAppDbContext _context;
    public UpdatePasajeHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(UpdatePasajeCommand request, CancellationToken token)
    {
        var pasaje = await _context.Pasajes.FindAsync(new object[] { request.Id }, token);
        if (pasaje == null) return false;

        pasaje.NombrePasajero = request.NombrePasajero;
        await _context.SaveChangesAsync(token);
        return true;
    }
}