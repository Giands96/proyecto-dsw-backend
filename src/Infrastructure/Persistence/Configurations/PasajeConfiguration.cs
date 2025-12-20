using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PasajeConfiguration : IEntityTypeConfiguration<Pasaje>
{
    public void Configure(EntityTypeBuilder<Pasaje> builder)
    {
        builder.ToTable("Pasajes");
        builder.HasKey(x => x.Id);
       //builder.Property(x => x.Id).HasColumnType("char(36)");
        //builder.Property(x => x.ViajeId).HasColumnType("char(36)");
        //builder.Property(x => x.UsuarioCompradorId).HasColumnType("char(36)");
        builder.Property(x => x.NombrePasajero).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Costo).HasPrecision(10, 2);
        builder.Property(x => x.QRData).HasColumnType("text");
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");

        builder.HasOne<Viaje>()
            .WithMany()
            .HasForeignKey(x => x.ViajeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioCompradorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ViajeId);
    }
}
