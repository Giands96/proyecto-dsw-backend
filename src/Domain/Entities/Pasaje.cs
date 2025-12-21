namespace Domain.Entities;

public class Pasaje
{
    public Guid Id { get; set; }
    public Guid ViajeId { get; set; }
    public Guid UsuarioCompradorId { get; set; }
    public string NombrePasajero { get; set; } = string.Empty;
    public decimal Costo { get; set; }
    public string QRData { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual Usuario? UsuarioComprador { get; set; }
    public virtual Viaje? Viaje { get; set; }
}

