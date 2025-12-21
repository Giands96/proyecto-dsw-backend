using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {

        if (await context.Usuarios.AnyAsync(u => u.Email == "admin@bus.com"))
        {
            return; 
        }
        var admin = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "Administrador Sistema",
            Email = "admin@bus.com",
            Rol = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            PasswordHash = passwordHasher.Hash("Admin123$") 
        };

        context.Usuarios.Add(admin);
        await context.SaveChangesAsync();
    }
}