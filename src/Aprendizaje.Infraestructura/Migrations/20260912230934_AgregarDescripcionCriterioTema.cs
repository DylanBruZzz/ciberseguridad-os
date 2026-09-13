using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aprendizaje.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDescripcionCriterioTema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                schema: "roadmap",
                table: "CriterioTema",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                schema: "roadmap",
                table: "CriterioTema");
        }
    }
}
