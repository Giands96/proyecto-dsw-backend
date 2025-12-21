using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPasajeFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pasajes_Usuarios_UsuarioCompradorId",
                table: "Pasajes");

            migrationBuilder.DropForeignKey(
                name: "FK_Pasajes_Usuarios_UsuarioCompradorId1",
                table: "Pasajes");

            migrationBuilder.DropForeignKey(
                name: "FK_Pasajes_Viajes_ViajeId1",
                table: "Pasajes");

            migrationBuilder.DropIndex(
                name: "IX_Pasajes_UsuarioCompradorId1",
                table: "Pasajes");

            migrationBuilder.DropIndex(
                name: "IX_Pasajes_ViajeId1",
                table: "Pasajes");

            migrationBuilder.DropColumn(
                name: "UsuarioCompradorId1",
                table: "Pasajes");

            migrationBuilder.DropColumn(
                name: "ViajeId1",
                table: "Pasajes");

            migrationBuilder.AddForeignKey(
                name: "FK_Pasajes_Usuarios_UsuarioCompradorId",
                table: "Pasajes",
                column: "UsuarioCompradorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pasajes_Usuarios_UsuarioCompradorId",
                table: "Pasajes");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCompradorId1",
                table: "Pasajes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ViajeId1",
                table: "Pasajes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pasajes_UsuarioCompradorId1",
                table: "Pasajes",
                column: "UsuarioCompradorId1");

            migrationBuilder.CreateIndex(
                name: "IX_Pasajes_ViajeId1",
                table: "Pasajes",
                column: "ViajeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Pasajes_Usuarios_UsuarioCompradorId",
                table: "Pasajes",
                column: "UsuarioCompradorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pasajes_Usuarios_UsuarioCompradorId1",
                table: "Pasajes",
                column: "UsuarioCompradorId1",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pasajes_Viajes_ViajeId1",
                table: "Pasajes",
                column: "ViajeId1",
                principalTable: "Viajes",
                principalColumn: "Id");
        }
    }
}
