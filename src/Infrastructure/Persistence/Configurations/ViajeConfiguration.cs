using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ViajeConfiguration : IEntityTypeConfiguration<Viaje>
{
    public void Configure(EntityTypeBuilder<Viaje> builder)
    {
        builder.ToTable("Viajes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Destino).IsRequired().HasMaxLength(120);
        builder.Property(x => x.FechaHora).HasColumnType("timestamp with time zone");
        builder.Property(x => x.CostoBase).HasPrecision(10, 2);
        builder.Property(x => x.CapacidadMax).HasDefaultValue(50);
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
