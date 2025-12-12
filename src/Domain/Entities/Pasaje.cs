namespace Domain.Entities;

public class Pasaje
{
    public Guid Id { get; set; }
    public Guid ViajeId { get; set; }
    public Guid UsuarioCompradorId { get; set; }
    public string NombrePasajero { get; set; } = string.Empty;
    public decimal Costo { get; set; }
    public string QRData { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
