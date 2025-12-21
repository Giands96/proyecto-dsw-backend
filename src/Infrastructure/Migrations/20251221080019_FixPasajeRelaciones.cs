using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPasajeRelaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Rol = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Viajes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Destino = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    FechaHora = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CostoBase = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CapacidadMax = table.Column<int>(type: "integer", nullable: false, defaultValue: 50),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Viajes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pasajes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ViajeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioCompradorId = table.Column<Guid>(type: "uuid", nullable: false),
                    NombrePasajero = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Costo = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    QRData = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UsuarioCompradorId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    ViajeId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pasajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pasajes_Usuarios_UsuarioCompradorId",
                        column: x => x.UsuarioCompradorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pasajes_Usuarios_UsuarioCompradorId1",
                        column: x => x.UsuarioCompradorId1,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Pasajes_Viajes_ViajeId",
                        column: x => x.ViajeId,
                        principalTable: "Viajes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pasajes_Viajes_ViajeId1",
                        column: x => x.ViajeId1,
                        principalTable: "Viajes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pasajes_UsuarioCompradorId",
                table: "Pasajes",
                column: "UsuarioCompradorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pasajes_UsuarioCompradorId1",
                table: "Pasajes",
                column: "UsuarioCompradorId1");

            migrationBuilder.CreateIndex(
                name: "IX_Pasajes_ViajeId",
                table: "Pasajes",
                column: "ViajeId");

            migrationBuilder.CreateIndex(
                name: "IX_Pasajes_ViajeId1",
                table: "Pasajes",
                column: "ViajeId1");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pasajes");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Viajes");
        }
    }
}
