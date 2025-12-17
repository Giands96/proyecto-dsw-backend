# Arquitectura BusTickets

## Visión General
- **Stack:** .NET 8 + MySQL + EF Core + CQRS/MediatR + Minimal API.
- **Patrón:** Clean Architecture. Cada capa solo depende de la interior.
- **Módulos principales:**
  - `Domain`: entidades y reglas básicas.
  - `Application`: casos de uso (commands/queries) y validaciones.
  - `Infrastructure`: acceso a datos, servicios externos, implementación de interfaces.
  - `Api`: capa de entrada HTTP (Minimal API) que configura DI, autenticación y endpoints.

## Flujo de una solicitud
1. **Http Request (Api):** llega al endpoint definido en `Program.cs`.
2. **MediatR:** el endpoint crea un comando/query y lo envía a través de `ISender`.
3. **Application Handler:** aplica validaciones (FluentValidation) vía `ValidationBehavior`, ejecuta lógica de negocio y decide qué repositorios/servicios usar.
4. **Infrastructure:** provee `AppDbContext` (EF Core MySQL) y servicios como hashing, JWT, QR, gateway de pago.
5. **Domain:** entidades (`Usuario`, `Viaje`, `Pasaje`) representan el modelo persistido.
6. **Respuesta:** el handler retorna DTOs (`AuthResponse`, `ViajeDto`, `PasajeCardDto`) que el endpoint serializa como JSON.

## Capas en detalle
### Domain
- Contiene entidades simples sin dependencias externas.
- Enum `UserRole` define roles (Invitado/Comprador).
- No referencia otras capas.

### Application
- Depende solo de Domain.
- Define interfaces (`IAppDbContext`, `IPaymentGateway`, `IQRGenerator`, `ITokenService`, `IPasswordHasher`).
- Implementa CQRS con MediatR y validaciones con FluentValidation.
- No conoce EF ni detalles de infraestructura; opera contra las interfaces.

### Infrastructure
- Implementa las interfaces de Application.
- `Persistence/AppDbContext` usa Pomelo MySQL; configura entidades mediante Fluent API.
- `Services/` contiene adapters (JWT, QR, Payment mock, PasswordHasher).
- Expone `AddInfrastructure` para registrar DbContext, servicios y cadena de conexión.

### Api
- Proyecto ASP.NET Core Minimal API.
- Configura Swagger, JWT Bearer, Serilog y registra las capas.
- Define endpoints RESTful (`/api/auth/*`, `/api/viajes`, `/api/pasajes`, `/api/user/mis-pasajes`, `/api/pasajes/validar`).
- Ejecuta `app.Run()` sin lógica adicional (thin controllers conforme a Clean Architecture).

## Bases de Datos y Migraciones
- MySQL 8.x a través de `Pomelo.EntityFrameworkCore.MySql`.
- `DesignTimeDbContextFactory` permite ejecutar `dotnet ef` para migraciones.
- Migraciones almacenadas en `Infrastructure/Migrations`.

## Autenticación
- JWT Bearer con claves configurables (`Jwt:*` en `appsettings`).
- JWT generado por `JwtTokenService` y validado en API; endpoints sensibles requieren `[RequireAuthorization]`.

## Extras
- **Frontend** (carpeta aparte) consume `/api` mediante axios.
- **Serilog** envía logs a consola, configurable para agregar sinks.

Esta arquitectura permite escalar funcionalidades añadiendo nuevos comandos/queries sin tocar la capa web, manteniendo separación de responsabilidades y testabilidad.
