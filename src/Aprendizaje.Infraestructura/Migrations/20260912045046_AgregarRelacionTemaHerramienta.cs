using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aprendizaje.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRelacionTemaHerramienta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TemaHerramienta",
                schema: "roadmap",
                columns: table => new
                {
                    TemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HerramientaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemaHerramienta", x => new { x.TemaId, x.HerramientaId });
                    table.ForeignKey(
                        name: "FK_TemaHerramienta_Herramienta_HerramientaId",
                        column: x => x.HerramientaId,
                        principalSchema: "study",
                        principalTable: "Herramienta",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TemaHerramienta_Tema_TemaId",
                        column: x => x.TemaId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemaHerramienta_HerramientaId",
                schema: "roadmap",
                table: "TemaHerramienta",
                column: "HerramientaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemaHerramienta",
                schema: "roadmap");
        }
    }
}
