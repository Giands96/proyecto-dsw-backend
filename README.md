# BusTickets API (.NET 8, Clean Architecture, MySQL)

## Estructura
- `Domain`: Entidades (`Usuario`, `Viaje`, `Pasaje`), enum `UserRole`.
- `Application`: CQRS con MediatR y FluentValidation. Casos:
  - Auth: register/login → JWT.
  - Viajes: listar y detalle.
  - Pasajes: comprar, listar propios (paginado 5), validar QR.
- `Infrastructure`: EF Core MySQL (Pomelo), AppDbContext, configuraciones, servicios (JWT, QR, pago mock, hasher).
- `Api`: Minimal API + Swagger + JWT auth.

## Configuración
1. MySQL 8.x corriendo. Crear DB `bus_tickets` o la que desees.
2. Ajusta `src/Api/appsettings.json`:
```json
"ConnectionStrings": {
  "Default": "Server=localhost;Port=3306;Database=bus_tickets;User=root;Password=pass;TreatTinyAsBoolean=true;DefaultCommandTimeout=60;"
},
"Jwt": {
  "Key": "supersecretkey1234567890",
  "Issuer": "bus-api",
  "Audience": "bus-api"
}
```
3. Migraciones (en PowerShell):
```
cd d:\proyecto-dsw
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet ef migrations add Init --project src\Infrastructure --startup-project src\Api --output-dir Persistence\Migrations
```
Luego aplicar:
```
dotnet ef database update --project src\Infrastructure --startup-project src\Api
```

## Run
```
dotnet run --project src\Api
```
Swagger: http://localhost:5000/swagger

## Endpoints principales
- POST `/api/auth/register` { nombre, email, password }
- POST `/api/auth/login` { email, password }
- GET `/api/viajes?destino=&desde=&hasta=&page=1&pageSize=20`
- GET `/api/viajes/{id}`
- POST `/api/pasajes` (JWT) body: { viajeId, pasajeros:[{ nombre }...] }
- GET `/api/pasajes/mis?page=1&pageSize=5` (JWT)
- POST `/api/pasajes/validar` { qrContent }

## Notas
- Capacidad de viaje: 50. Costo por pasaje = `Viaje.CostoBase`.
- Pago simulado en `MockPaymentGateway` (si quieres fallas, ajusta ahí).
- QR generado en base64 PNG con QRCoder (usa System.Drawing; en non-Windows puede requerir librerías GDI+).
- Al arrancar se ejecuta `Database.Migrate()`.
