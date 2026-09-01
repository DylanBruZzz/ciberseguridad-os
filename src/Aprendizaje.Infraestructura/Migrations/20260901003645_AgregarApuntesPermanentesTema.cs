using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aprendizaje.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarApuntesPermanentesTema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApunteTema",
                schema: "roadmap",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApunteTema", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApunteTema_Tema_TemaId",
                        column: x => x.TemaId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApunteTema_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApunteTema_TemaId",
                schema: "roadmap",
                table: "ApunteTema",
                column: "TemaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApunteTema_UsuarioId",
                schema: "roadmap",
                table: "ApunteTema",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApunteTema",
                schema: "roadmap");
        }
    }
}
