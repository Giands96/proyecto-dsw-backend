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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddAuthorization();

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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

app.MapGet("/api/viajes/{id}", async (Guid id, ISender sender) =>
{
    var result = await sender.Send(new GetViajeByIdQuery(id));
    return result is null ? Results.NotFound() : Results.Ok(result);
});

app.MapPost("/api/pasajes", async (ComprarPasajesRequest request, ClaimsPrincipal user, ISender sender) =>
{
    var subject = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!Guid.TryParse(subject, out var userId))
        return Results.Unauthorized();

    var cmd = new ComprarPasajesCommand(request.ViajeId, request.Pasajeros.Select(p => new PasajeroInput(p.Nombre)).ToList(), userId);
    var result = await sender.Send(cmd);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapGet("/api/pasajes/mis", async (int page, int pageSize, ClaimsPrincipal user, ISender sender) =>
{
    var subject = user.FindFirstValue("sub");
    if (!Guid.TryParse(subject, out var userId))
        return Results.Unauthorized();

    var query = new GetPasajesByUsuarioQuery(userId, page == 0 ? 1 : page, pageSize == 0 ? 5 : pageSize);
    var result = await sender.Send(query);
    return Results.Ok(result);
}).RequireAuthorization();

app.MapPost("/api/pasajes/validar", async (ValidarPasajeRequest request, ISender sender) =>
{
    var result = await sender.Send(new ValidarPasajeQuery(request.QrContent));
    return Results.Ok(result);
});

app.MapGet("/health", () => Results.Ok("ok"));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

public record ComprarPasajesRequest(Guid ViajeId, List<PasajeroRequest> Pasajeros);
public record PasajeroRequest(string Nombre);
public record ValidarPasajeRequest(string QrContent);
