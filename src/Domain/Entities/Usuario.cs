using Domain.Enums;

namespace Domain.Entities;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Rol { get; set; } = UserRole.Invitado;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
