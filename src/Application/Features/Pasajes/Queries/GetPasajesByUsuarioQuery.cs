using Application.Common.Interfaces;
using Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Pasajes.Queries;

public record GetPasajesByUsuarioQuery(Guid UsuarioId, int Page = 1, int PageSize = 5) : IRequest<PagedPasajesResponse>;

public record PagedPasajesResponse(IReadOnlyList<PasajeCardDto> Items, int Total, int Page, int PageSize);

public class GetPasajesByUsuarioQueryHandler : IRequestHandler<GetPasajesByUsuarioQuery, PagedPasajesResponse>
{
    private readonly IAppDbContext _context;

    public GetPasajesByUsuarioQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedPasajesResponse> Handle(GetPasajesByUsuarioQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = _context.Pasajes.Where(p => p.UsuarioCompradorId == request.UsuarioId);
        var total = await baseQuery.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var pasajes = await baseQuery
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = pasajes.Select(p => new PasajeCardDto(p.Id, p.NombrePasajero, p.Costo, p.QRData)).ToList();
        return new PagedPasajesResponse(items, total, request.Page, request.PageSize);
    }
}
