using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aprendizaje.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMetadataPedagogicaFase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CargaSemanalRecomendada",
                schema: "roadmap",
                table: "Fase",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CriteriosAvance",
                schema: "roadmap",
                table: "Fase",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MesFinRecomendado",
                schema: "roadmap",
                table: "Fase",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MesInicioRecomendado",
                schema: "roadmap",
                table: "Fase",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Objetivos",
                schema: "roadmap",
                table: "Fase",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Fase_MesesRecomendados",
                schema: "roadmap",
                table: "Fase",
                sql: "[MesInicioRecomendado] IS NULL OR [MesFinRecomendado] IS NULL OR [MesFinRecomendado] >= [MesInicioRecomendado]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Fase_MesFinRecomendado",
                schema: "roadmap",
                table: "Fase",
                sql: "[MesFinRecomendado] IS NULL OR [MesFinRecomendado] >= 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Fase_MesInicioRecomendado",
                schema: "roadmap",
                table: "Fase",
                sql: "[MesInicioRecomendado] IS NULL OR [MesInicioRecomendado] >= 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Fase_MesesRecomendados",
                schema: "roadmap",
                table: "Fase");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Fase_MesFinRecomendado",
                schema: "roadmap",
                table: "Fase");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Fase_MesInicioRecomendado",
                schema: "roadmap",
                table: "Fase");

            migrationBuilder.DropColumn(
                name: "CargaSemanalRecomendada",
                schema: "roadmap",
                table: "Fase");

            migrationBuilder.DropColumn(
                name: "CriteriosAvance",
                schema: "roadmap",
                table: "Fase");

            migrationBuilder.DropColumn(
                name: "MesFinRecomendado",
                schema: "roadmap",
                table: "Fase");

            migrationBuilder.DropColumn(
                name: "MesInicioRecomendado",
                schema: "roadmap",
                table: "Fase");

            migrationBuilder.DropColumn(
                name: "Objetivos",
                schema: "roadmap",
                table: "Fase");
        }
    }
}
