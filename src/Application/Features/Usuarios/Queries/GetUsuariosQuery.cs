using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Usuarios.Queries;

public record UsuarioDto(Guid Id, string Nombre, string Email, string Rol);
public record PaginatedUsuariosDto(List<UsuarioDto> Items, int TotalCount, int Page, int PageSize);
public record GetUsuariosQuery(int Page = 1, int PageSize = 10) : IRequest<PaginatedUsuariosDto>;

public class GetUsuariosHandler : IRequestHandler<GetUsuariosQuery, PaginatedUsuariosDto>
{
    private readonly IAppDbContext _context;
    public GetUsuariosHandler(IAppDbContext context) => _context = context;

    public async Task<PaginatedUsuariosDto> Handle(GetUsuariosQuery request, CancellationToken token)
    {
        var query = _context.Usuarios.AsQueryable();
        var total = await query.CountAsync(token);

        var entities = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(token);

        var dtos = entities.Select(u => new UsuarioDto(
            u.Id, u.Nombre, u.Email, u.Rol.ToString()
        )).ToList();

        return new PaginatedUsuariosDto(dtos, total, request.Page, request.PageSize);
    }
}