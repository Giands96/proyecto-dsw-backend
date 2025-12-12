namespace Application.Dtos;

public record AuthResponse(string Token, string Email, string Nombre, string Rol);
