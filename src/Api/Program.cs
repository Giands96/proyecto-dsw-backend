using System.Security.Claims;
using Application;
using Application.Features.Auth;
using Application.Features.Pasajes.Commands;
using Application.Features.Pasajes.Queries;
using Application.Features.Viajes.Queries;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using MediatR;
using Microsoft.OpenApi.Models;
using Application.Features.Viajes.Commands;
using Application.Features.Usuarios.Queries;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        var corsOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ;

        policy.WithOrigins(corsOrigins!)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BusTickets API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"Ingresar 'Bearer' y luego el token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
    

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"] ?? "supersecretkey1234567890";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

//builder.Services.AddAuthorization();

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => 
        policy.RequireRole("Admin"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    { 
        var context = services.GetRequiredService<Infrastructure.Persistence.AppDbContext>();
        var hasher = services.GetRequiredService<Application.Common.Interfaces.IPasswordHasher>();
        // Retry DB connection on container startup while Postgres is initializing.
        var maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                await Infrastructure.Persistence.DbInitializer.InitializeAsync(context, hasher);
                break;
            }
            catch when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al crear el Admin por defecto.");
    }
}

app.UseMiddleware<Api.Middlewares.GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/register", async (RegisterUserCommand cmd, ISender sender) =>
{
    var result = await sender.Send(cmd);
    return Results.Ok(result);
});

app.MapPost("/api/auth/login", async (LoginCommand cmd, ISender sender) =>
{
    var result = await sender.Send(cmd);
    return Results.Ok(result);
});

app.MapGet("/api/viajes", async (string? destino, DateTimeOffset? desde, DateTimeOffset? hasta, int page, int pageSize, ISender sender) =>
{
    var query = new GetViajesQuery(destino, desde, hasta, page == 0 ? 1 : page, pageSize == 0 ? 20 : pageSize);
    var result = await sender.Send(query);
    return Results.Ok(result);
});

app.MapGet("/api/viajes/destinos", async (string? destino, DateTimeOffset? desde, DateTimeOffset? hasta, int page, int pageSize, ISender sender) =>
{
    var query = new GetViajesQuery(destino, desde, hasta, page == 0 ? 1 : page, pageSize == 0 ? 20 : pageSize);
    var result = await sender.Send(query);
    return Results.Ok(result);
});

app.MapGet("/api/viajes/{id}", async (Guid id, ISender sender) =>
{
    var result = await sender.Send(new GetViajeByIdQuery(id));
    return result is null ? Results.NotFound() : Results.Ok(result);
});

app.MapPost("/api/pasajes", async (ComprarPasajesRequest request, ClaimsPrincipal user, ISender sender) =>
{
    var subject = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
    if (!Guid.TryParse(subject, out var userId))
        return Results.Unauthorized();

    var cmd = new ComprarPasajesCommand(request.ViajeId, request.Pasajeros.Select(p => new PasajeroInput(p.Nombre)).ToList(), userId);
    var result = await sender.Send(cmd);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapGet("/api/pasajes/mis", async (int? page, int? pageSize, ClaimsPrincipal user, ISender sender) =>
{
    var subject = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
    if (!Guid.TryParse(subject, out var userId))
        return Results.Unauthorized();
    var query = new GetPasajesByUsuarioQuery(
        userId, 
        page ?? 1, 
        pageSize ?? 10
    );
    
    var result = await sender.Send(query);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapPost("/api/pasajes/validar", async (ValidarPasajeRequest request, ISender sender) =>
{
    var result = await sender.Send(new ValidarPasajeQuery(request.QrContent));
    return Results.Ok(result);
});

// Admin Group

var adminGroup = app.MapGroup("/api/admin").RequireAuthorization("AdminPolicy"); 

// --- VIAJES ---
adminGroup.MapPost("/viajes", async (CreateViajeCommand command, ISender sender) =>
{
    var id = await sender.Send(command);
    return Results.Created($"/api/viajes/{id}", id);
});

adminGroup.MapPut("/viajes/{id}", async (Guid id, UpdateViajeCommand command, ISender sender) =>
{
    if (id != command.Id) return Results.BadRequest();
    var success = await sender.Send(command);
    return success ? Results.NoContent() : Results.NotFound();
});

adminGroup.MapDelete("/viajes/{id}", async (Guid id, ISender sender) =>
{
    var success = await sender.Send(new DeleteViajeCommand(id));
    return success ? Results.NoContent() : Results.NotFound();
});

// --- USUARIOS ---
adminGroup.MapGet("/usuarios", async (int? page, int? pageSize, ISender sender) =>
{
    var query = new GetUsuariosQuery(page ?? 1, pageSize ?? 10);
    var result = await sender.Send(query);
    return Results.Ok(result);
});

// -- PASAJES ---
adminGroup.MapGet("/viajes/{viajeId}/pasajes", async (Guid viajeId, ISender sender) =>
{
    var result = await sender.Send(new GetPasajesByViajeIdQuery(viajeId));
    return Results.Ok(result);
});

adminGroup.MapPut("/pasajes/{id}", async (Guid id, UpdatePasajeCommand command, ISender sender) =>
{
    if (id != command.Id) return Results.BadRequest();
    var success = await sender.Send(command);
    return success ? Results.NoContent() : Results.NotFound();
});

adminGroup.MapDelete("/pasajes/{id}", async (Guid id, ISender sender) =>
{
    var success = await sender.Send(new DeletePasajeCommand(id));
    return success ? Results.NoContent() : Results.NotFound();
});

app.Run();

public record ComprarPasajesRequest(Guid ViajeId, List<PasajeroRequest> Pasajeros);
public record PasajeroRequest(string Nombre);
public record ValidarPasajeRequest(string QrContent);
