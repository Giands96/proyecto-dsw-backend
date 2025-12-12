namespace Domain.Entities;

public class Viaje
{
    public Guid Id { get; set; }
    public string Destino { get; set; } = string.Empty;
    public DateTimeOffset FechaHora { get; set; }
    public decimal CostoBase { get; set; }
    public int CapacidadMax { get; set; } = 50;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int AsientosDisponibles(int vendidos) => Math.Max(0, CapacidadMax - vendidos);
}
