namespace Application.Dtos;

public record ViajeDto(Guid Id, string Destino, DateTimeOffset FechaHora, decimal CostoBase, int AsientosDisponibles);
