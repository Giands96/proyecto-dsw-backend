using Application.Common.Interfaces;
using Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user is null)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        if (!_passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        var token = _tokenService.CreateToken(user);
        return new AuthResponse(token, user.Email, user.Nombre, user.Rol.ToString());
    }
}
