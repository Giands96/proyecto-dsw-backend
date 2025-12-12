using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Usuario> Usuarios { get; }
    DbSet<Viaje> Viajes { get; }
    DbSet<Pasaje> Pasajes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
