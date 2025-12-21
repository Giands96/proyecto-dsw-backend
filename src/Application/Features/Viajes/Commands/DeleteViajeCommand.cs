using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Viajes.Commands;

public record DeleteViajeCommand(Guid Id) : IRequest<bool>;

public class DeleteViajeHandler : IRequestHandler<DeleteViajeCommand, bool>
{
    private readonly IAppDbContext _context;

    public DeleteViajeHandler(IAppDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteViajeCommand request, CancellationToken token)
    {
        var entity = await _context.Viajes.FindAsync(new object[] { request.Id }, token);
        if (entity == null) return false;

        _context.Viajes.Remove(entity);
        await _context.SaveChangesAsync(token);
        return true;
    }
}