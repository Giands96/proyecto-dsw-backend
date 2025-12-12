using Application.Common.Interfaces;
using Application.Dtos;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth;

public record RegisterUserCommand(string Nombre, string Email, string Password) : IRequest<AuthResponse>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public RegisterUserCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Usuarios.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (exists)
            throw new InvalidOperationException("El email ya está registrado.");

        var user = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Rol = UserRole.Comprador,
            CreatedAt = DateTime.UtcNow
        };

        _context.Usuarios.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var token = _tokenService.CreateToken(user);
        return new AuthResponse(token, user.Email, user.Nombre, user.Rol.ToString());
    }
}
