using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Pasajes.Commands;

public record DeletePasajeCommand(Guid Id) : IRequest<bool>;

public class DeletePasajeHandler : IRequestHandler<DeletePasajeCommand, bool>
{
    private readonly IAppDbContext _context;
    public DeletePasajeHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(DeletePasajeCommand request, CancellationToken token)
    {
        var pasaje = await _context.Pasajes.FindAsync(new object[] { request.Id }, token);
        if (pasaje == null) return false;

        _context.Pasajes.Remove(pasaje);
        await _context.SaveChangesAsync(token);
        return true;
    }
}