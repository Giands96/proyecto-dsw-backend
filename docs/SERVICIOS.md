# Procesos de Servicios

Este documento describe el rol y flujo de cada servicio relevante dentro del backend BusTickets.

## Infrastructure.Services

### PasswordHasher
- **Entrada:** texto plano enviado desde comandos de Auth.
- **Proceso:** utiliza BCrypt Enhanced Hash para generar/validar hash.
- **Salida:**
  - `Hash(string password)`: devuelve hash para persistir en `Usuario.PasswordHash`.
  - `Verify(string hash, string password)`: confirma credenciales durante login.

### JwtTokenService
- **Entrada:** entidad `Usuario` autenticada.
- **Proceso:**
  1. Obtiene `Jwt:Key`, `Issuer` y `Audience` desde configuración.
  2. Construye claims (`sub`, `email`, `role`, `name`).
  3. Firma un JWT HS256 válido por 6 horas.
- **Salida:** string JWT utilizado por el frontend para autenticación.

### MockPaymentGateway
- **Entrada:** monto total de la compra.
- **Proceso:** simula integración de pago externa aprobando siempre la solicitud (puede extenderse para escenarios de error).
- **Salida:** `PaymentResult` con `Success=true` y referencia ficticia.

### QRCoderGenerator
- **Entrada:** payload (normalmente `Pasaje.Id`).
- **Proceso:**
  1. Genera QR con QRCoder y ECCLevel Q.
  2. Renderiza PNG en memoria.
  3. Devuelve la imagen codificada en Base64.
- **Salida:** string base64 persistido en `Pasaje.QRData` y devuelto en las cards.

## Infrastructure.Persistence

### AppDbContext
- **Entrada:** solicitudes de data desde Application (commands/queries).
- **Proceso:**
  - Expone `DbSet<Usuario>`, `DbSet<Viaje>`, `DbSet<Pasaje>`.
  - Aplica configuraciones de entidades.
  - Gestiona transacciones y `SaveChangesAsync`.
- **Salida:** Objeto de contexto disponible para los handlers.

### DesignTimeDbContextFactory
- **Uso específico:** tooling EF (`dotnet ef`).
- **Proceso:** construye `AppDbContext` leyendo conexión desde `appsettings`/variables.

## Application (Servicios lógicos via MediatR)

### Auth Handlers
- **RegisterUserCommandHandler:**
  1. Valida email único.
  2. Hashea password.
  3. Guarda usuario y emite JWT.
- **LoginCommandHandler:** verifica credenciales y genera JWT.

### Viajes Queries
- `GetViajesQueryHandler`: filtra viajes (destino/fechas), calcula asientos disponibles consultando `Pasajes`.
- `GetViajeByIdQueryHandler`: retorna detalle con disponibilidad.

### Pasajes Commands/Queries
- `ComprarPasajesCommandHandler`:
  1. Valida viaje + capacidad.
  2. Cobra vía `IPaymentGateway`.
  3. Genera QR y crea pasajes (uno por pasajero).
- `GetPasajesByUsuarioQueryHandler`: pagina cards del usuario autenticado.
- `ValidarPasajeQueryHandler`: confirma validez de un QR (ID válido y asociación a viaje vigente).

Esta capa se apoya en los servicios de Infrastructure mediante las interfaces definidas en `Application.Common.Interfaces`.
