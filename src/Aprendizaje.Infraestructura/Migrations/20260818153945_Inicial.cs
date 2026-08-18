using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aprendizaje.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "evidence");

            migrationBuilder.EnsureSchema(
                name: "roadmap");

            migrationBuilder.EnsureSchema(
                name: "integration");

            migrationBuilder.EnsureSchema(
                name: "study");

            migrationBuilder.EnsureSchema(
                name: "resource");

            migrationBuilder.EnsureSchema(
                name: "analytics");

            migrationBuilder.EnsureSchema(
                name: "nucleo");

            migrationBuilder.CreateTable(
                name: "Certificacion",
                schema: "roadmap",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Proveedor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TipoCosto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificacion", x => x.Id);
                    table.CheckConstraint("CK_Certificacion_TipoCosto", "[TipoCosto] IN ('Gratuita','Pago')");
                });

            migrationBuilder.CreateTable(
                name: "Herramienta",
                schema: "study",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Herramienta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                schema: "nucleo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IntervaloRepasoDefectoDias = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    CertificacionObjetivoActivaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    FechaEliminacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuario_Certificacion_CertificacionObjetivoActivaId",
                        column: x => x.CertificacionObjetivoActivaId,
                        principalSchema: "roadmap",
                        principalTable: "Certificacion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ArtefactoTecnico",
                schema: "evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoArtefacto = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContenidoOUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LenguajeTecnologia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EstadoMadurez = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'Borrador'"),
                    FechaEliminacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtefactoTecnico", x => x.Id);
                    table.CheckConstraint("CK_ArtefactoTecnico_EstadoMadurez", "[EstadoMadurez] IN ('Borrador','Documentado','ListoPortafolio','Publicado')");
                    table.CheckConstraint("CK_ArtefactoTecnico_Tipo", "[TipoArtefacto] IN ('Script','Herramienta','Cheatsheet','Dashboard','Playbook','ReglaDeteccion','ConsultaSiem','Automatizacion','Plantilla','Otro')");
                    table.ForeignKey(
                        name: "FK_ArtefactoTecnico_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CertificacionObtenida",
                schema: "evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CertificacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaObtencion = table.Column<DateOnly>(type: "date", nullable: false),
                    EvidenciaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EstadoMadurez = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'Documentado'"),
                    FechaEliminacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificacionObtenida", x => x.Id);
                    table.CheckConstraint("CK_CertificacionObtenida_EstadoMadurez", "[EstadoMadurez] IN ('Borrador','Documentado','ListoPortafolio','Publicado')");
                    table.ForeignKey(
                        name: "FK_CertificacionObtenida_Certificacion_CertificacionId",
                        column: x => x.CertificacionId,
                        principalSchema: "roadmap",
                        principalTable: "Certificacion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CertificacionObtenida_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Competencia",
                schema: "roadmap",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Competencia_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Conector",
                schema: "integration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Plataforma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EstadoConexion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'Desconectado'"),
                    UltimaSincronizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CredencialRef = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conector", x => x.Id);
                    table.CheckConstraint("CK_Conector_Estado", "[EstadoConexion] IN ('Conectado','Desconectado','Error')");
                    table.CheckConstraint("CK_Conector_Plataforma", "[Plataforma] IN ('GitHub','TryHackMe','HackTheBox','NotebookLM','Otro')");
                    table.ForeignKey(
                        name: "FK_Conector_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Fase",
                schema: "roadmap",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fase_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Laboratorio",
                schema: "evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Objetivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EntornoVms = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Hallazgos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TiempoInvertidoMinutos = table.Column<int>(type: "int", nullable: true),
                    EstadoMadurez = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'Borrador'"),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaEliminacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Laboratorio", x => x.Id);
                    table.CheckConstraint("CK_Laboratorio_EstadoMadurez", "[EstadoMadurez] IN ('Borrador','Documentado','ListoPortafolio','Publicado')");
                    table.ForeignKey(
                        name: "FK_Laboratorio_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Proyecto",
                schema: "evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'Idea'"),
                    EstadoMadurez = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'Borrador'"),
                    RepositorioUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    FechaEliminacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proyecto", x => x.Id);
                    table.CheckConstraint("CK_Proyecto_Estado", "[Estado] IN ('Idea','Desarrollo','Documentado','Publicado')");
                    table.CheckConstraint("CK_Proyecto_EstadoMadurez", "[EstadoMadurez] IN ('Borrador','Documentado','ListoPortafolio','Publicado')");
                    table.ForeignKey(
                        name: "FK_Proyecto_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Recurso",
                schema: "resource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'PorClasificar'"),
                    Rating = table.Column<byte>(type: "tinyint", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    HerramientaIA = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PromptsUtilizados = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaEliminacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recurso", x => x.Id);
                    table.CheckConstraint("CK_Recurso_Estado", "[Estado] IN ('PorClasificar','PorRevisar','EnUso','Consultado','Referencia')");
                    table.CheckConstraint("CK_Recurso_Rating", "[Rating] IS NULL OR [Rating] BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_Recurso_Tipo", "[Tipo] IN ('Documentacion','Libro','Curso','Video','Laboratorio','Writeup','Cheatsheet','Script','RepositorioGitHub','NotebookIA','Otro')");
                    table.ForeignKey(
                        name: "FK_Recurso_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SnapshotProgreso",
                schema: "analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    PorcentajeGlobal = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    HorasTotales = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TemasDominados = table.Column<int>(type: "int", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnapshotProgreso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SnapshotProgreso_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Writeup",
                schema: "evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PlataformaOrigen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EstadoMadurez = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'Borrador'"),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaEliminacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Writeup", x => x.Id);
                    table.CheckConstraint("CK_Writeup_EstadoMadurez", "[EstadoMadurez] IN ('Borrador','Documentado','ListoPortafolio','Publicado')");
                    table.ForeignKey(
                        name: "FK_Writeup_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ArtefactoHerramienta",
                schema: "evidence",
                columns: table => new
                {
                    ArtefactoTecnicoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HerramientaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtefactoHerramienta", x => new { x.ArtefactoTecnicoId, x.HerramientaId });
                    table.ForeignKey(
                        name: "FK_ArtefactoHerramienta_ArtefactoTecnico_ArtefactoTecnicoId",
                        column: x => x.ArtefactoTecnicoId,
                        principalSchema: "evidence",
                        principalTable: "ArtefactoTecnico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtefactoHerramienta_Herramienta_HerramientaId",
                        column: x => x.HerramientaId,
                        principalSchema: "study",
                        principalTable: "Herramienta",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LogSincronizacion",
                schema: "integration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Resultado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Resumen = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogSincronizacion", x => x.Id);
                    table.CheckConstraint("CK_LogSincronizacion_Resultado", "[Resultado] IN ('Exito','Error','Parcial')");
                    table.ForeignKey(
                        name: "FK_LogSincronizacion_Conector_ConectorId",
                        column: x => x.ConectorId,
                        principalSchema: "integration",
                        principalTable: "Conector",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tema",
                schema: "roadmap",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TemaPadreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoConocimiento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    DificultadPercibida = table.Column<byte>(type: "tinyint", nullable: true),
                    Confianza = table.Column<byte>(type: "tinyint", nullable: true),
                    IntervaloRepasoDias = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    FechaEliminacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Objetivos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tema", x => x.Id);
                    table.CheckConstraint("CK_Tema_Confianza", "[Confianza] IS NULL OR [Confianza] BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_Tema_Dificultad", "[DificultadPercibida] IS NULL OR [DificultadPercibida] BETWEEN 1 AND 5");
                    table.CheckConstraint("CK_Tema_NoAutoPadre", "[TemaPadreId] IS NULL OR [TemaPadreId] <> [Id]");
                    table.CheckConstraint("CK_Tema_TipoConocimiento", "[TipoConocimiento] IN ('Conceptual','Procedimental','Herramienta')");
                    table.ForeignKey(
                        name: "FK_Tema_Fase_FaseId",
                        column: x => x.FaseId,
                        principalSchema: "roadmap",
                        principalTable: "Fase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tema_Tema_TemaPadreId",
                        column: x => x.TemaPadreId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tema_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LaboratorioHerramienta",
                schema: "evidence",
                columns: table => new
                {
                    LaboratorioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HerramientaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaboratorioHerramienta", x => new { x.LaboratorioId, x.HerramientaId });
                    table.ForeignKey(
                        name: "FK_LaboratorioHerramienta_Herramienta_HerramientaId",
                        column: x => x.HerramientaId,
                        principalSchema: "study",
                        principalTable: "Herramienta",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LaboratorioHerramienta_Laboratorio_LaboratorioId",
                        column: x => x.LaboratorioId,
                        principalSchema: "evidence",
                        principalTable: "Laboratorio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProyectoHerramienta",
                schema: "evidence",
                columns: table => new
                {
                    ProyectoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HerramientaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProyectoHerramienta", x => new { x.ProyectoId, x.HerramientaId });
                    table.ForeignKey(
                        name: "FK_ProyectoHerramienta_Herramienta_HerramientaId",
                        column: x => x.HerramientaId,
                        principalSchema: "study",
                        principalTable: "Herramienta",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProyectoHerramienta_Proyecto_ProyectoId",
                        column: x => x.ProyectoId,
                        principalSchema: "evidence",
                        principalTable: "Proyecto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtefactoTema",
                schema: "evidence",
                columns: table => new
                {
                    ArtefactoTecnicoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtefactoTema", x => new { x.ArtefactoTecnicoId, x.TemaId });
                    table.ForeignKey(
                        name: "FK_ArtefactoTema_ArtefactoTecnico_ArtefactoTecnicoId",
                        column: x => x.ArtefactoTecnicoId,
                        principalSchema: "evidence",
                        principalTable: "ArtefactoTecnico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtefactoTema_Tema_TemaId",
                        column: x => x.TemaId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CertificacionTema",
                schema: "roadmap",
                columns: table => new
                {
                    CertificacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Peso = table.Column<decimal>(type: "decimal(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificacionTema", x => new { x.CertificacionId, x.TemaId });
                    table.ForeignKey(
                        name: "FK_CertificacionTema_Certificacion_CertificacionId",
                        column: x => x.CertificacionId,
                        principalSchema: "roadmap",
                        principalTable: "Certificacion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CertificacionTema_Tema_TemaId",
                        column: x => x.TemaId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CompetenciaTema",
                schema: "roadmap",
                columns: table => new
                {
                    CompetenciaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetenciaTema", x => new { x.CompetenciaId, x.TemaId });
                    table.ForeignKey(
                        name: "FK_CompetenciaTema_Competencia_CompetenciaId",
                        column: x => x.CompetenciaId,
                        principalSchema: "roadmap",
                        principalTable: "Competencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetenciaTema_Tema_TemaId",
                        column: x => x.TemaId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CriterioTema",
                schema: "roadmap",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoCriterio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cumplido = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaCumplido = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriterioTema", x => x.Id);
                    table.CheckConstraint("CK_CriterioTema_Tipo", "[TipoCriterio] IN ('Teoria','Practica','Explicacion','Ejercicios','Laboratorio')");
                    table.ForeignKey(
                        name: "FK_CriterioTema_Tema_TemaId",
                        column: x => x.TemaId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntradaBitacora",
                schema: "study",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Texto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaEliminacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradaBitacora", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntradaBitacora_Tema_TemaId",
                        column: x => x.TemaId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EntradaBitacora_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LaboratorioTema",
                schema: "evidence",
                columns: table => new
                {
                    LaboratorioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaboratorioTema", x => new { x.LaboratorioId, x.TemaId });
                    table.ForeignKey(
                        name: "FK_LaboratorioTema_Laboratorio_LaboratorioId",
                        column: x => x.LaboratorioId,
                        principalSchema: "evidence",
                        principalTable: "Laboratorio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LaboratorioTema_Tema_TemaId",
                        column: x => x.TemaId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Nota",
                schema: "evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProyectoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LaboratorioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WriteupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArtefactoTecnicoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Texto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaEliminacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nota", x => x.Id);
                    table.CheckConstraint("CK_Nota_Tipo", "[Tipo] IN ('Nota','Hallazgo','Actualizacion','Autoexplicacion')");
                    table.CheckConstraint("CK_Nota_UnSoloPadre", "(CASE WHEN [TemaId] IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN [ProyectoId] IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN [LaboratorioId] IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN [WriteupId] IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN [ArtefactoTecnicoId] IS NOT NULL THEN 1 ELSE 0 END) = 1");
                    table.ForeignKey(
                        name: "FK_Nota_ArtefactoTecnico_ArtefactoTecnicoId",
                        column: x => x.ArtefactoTecnicoId,
                        principalSchema: "evidence",
                        principalTable: "ArtefactoTecnico",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Nota_Laboratorio_LaboratorioId",
                        column: x => x.LaboratorioId,
                        principalSchema: "evidence",
                        principalTable: "Laboratorio",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Nota_Proyecto_ProyectoId",
                        column: x => x.ProyectoId,
                        principalSchema: "evidence",
                        principalTable: "Proyecto",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Nota_Tema_TemaId",
                        column: x => x.TemaId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Nota_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Nota_Writeup_WriteupId",
                        column: x => x.WriteupId,
                        principalSchema: "evidence",
                        principalTable: "Writeup",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProyectoTema",
                schema: "evidence",
                columns: table => new
                {
                    ProyectoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProyectoTema", x => new { x.ProyectoId, x.TemaId });
                    table.ForeignKey(
                        name: "FK_ProyectoTema_Proyecto_ProyectoId",
                        column: x => x.ProyectoId,
                        principalSchema: "evidence",
                        principalTable: "Proyecto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProyectoTema_Tema_TemaId",
                        column: x => x.TemaId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RecursoTema",
                schema: "resource",
                columns: table => new
                {
                    RecursoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursoTema", x => new { x.RecursoId, x.TemaId });
                    table.ForeignKey(
                        name: "FK_RecursoTema_Recurso_RecursoId",
                        column: x => x.RecursoId,
                        principalSchema: "resource",
                        principalTable: "Recurso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecursoTema_Tema_TemaId",
                        column: x => x.TemaId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SesionEstudio",
                schema: "study",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    FechaEliminacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCreacionUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaModificacionUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionEstudio", x => x.Id);
                    table.CheckConstraint("CK_SesionEstudio_Duracion", "[DuracionMinutos] > 0");
                    table.CheckConstraint("CK_SesionEstudio_Tipo", "[Tipo] IN ('Teoria','Practica','Laboratorio','Repaso')");
                    table.ForeignKey(
                        name: "FK_SesionEstudio_Tema_TemaId",
                        column: x => x.TemaId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SesionEstudio_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "nucleo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TemaDependencia",
                schema: "roadmap",
                columns: table => new
                {
                    TemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemaRequisitoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemaDependencia", x => new { x.TemaId, x.TemaRequisitoId });
                    table.CheckConstraint("CK_TemaDependencia_NoAutoDependencia", "[TemaId] <> [TemaRequisitoId]");
                    table.ForeignKey(
                        name: "FK_TemaDependencia_Tema_TemaId",
                        column: x => x.TemaId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TemaDependencia_Tema_TemaRequisitoId",
                        column: x => x.TemaRequisitoId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WriteupTema",
                schema: "evidence",
                columns: table => new
                {
                    WriteupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WriteupTema", x => new { x.WriteupId, x.TemaId });
                    table.ForeignKey(
                        name: "FK_WriteupTema_Tema_TemaId",
                        column: x => x.TemaId,
                        principalSchema: "roadmap",
                        principalTable: "Tema",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WriteupTema_Writeup_WriteupId",
                        column: x => x.WriteupId,
                        principalSchema: "evidence",
                        principalTable: "Writeup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SesionHerramienta",
                schema: "study",
                columns: table => new
                {
                    SesionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HerramientaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionHerramienta", x => new { x.SesionId, x.HerramientaId });
                    table.ForeignKey(
                        name: "FK_SesionHerramienta_Herramienta_HerramientaId",
                        column: x => x.HerramientaId,
                        principalSchema: "study",
                        principalTable: "Herramienta",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SesionHerramienta_SesionEstudio_SesionId",
                        column: x => x.SesionId,
                        principalSchema: "study",
                        principalTable: "SesionEstudio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArtefactoHerramienta_HerramientaId",
                schema: "evidence",
                table: "ArtefactoHerramienta",
                column: "HerramientaId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtefactoTecnico_UsuarioId",
                schema: "evidence",
                table: "ArtefactoTecnico",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtefactoTema_TemaId",
                schema: "evidence",
                table: "ArtefactoTema",
                column: "TemaId");

            migrationBuilder.CreateIndex(
                name: "UQ_Certificacion_Nombre",
                schema: "roadmap",
                table: "Certificacion",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CertificacionObtenida_CertificacionId",
                schema: "evidence",
                table: "CertificacionObtenida",
                column: "CertificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificacionObtenida_UsuarioId",
                schema: "evidence",
                table: "CertificacionObtenida",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificacionTema_TemaId",
                schema: "roadmap",
                table: "CertificacionTema",
                column: "TemaId");

            migrationBuilder.CreateIndex(
                name: "UQ_Competencia_Usuario_Nombre",
                schema: "roadmap",
                table: "Competencia",
                columns: new[] { "UsuarioId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetenciaTema_TemaId",
                schema: "roadmap",
                table: "CompetenciaTema",
                column: "TemaId");

            migrationBuilder.CreateIndex(
                name: "UQ_Conector_Usuario_Plataforma",
                schema: "integration",
                table: "Conector",
                columns: new[] { "UsuarioId", "Plataforma" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CriterioTema_TemaId",
                schema: "roadmap",
                table: "CriterioTema",
                column: "TemaId");

            migrationBuilder.CreateIndex(
                name: "UQ_CriterioTema_TemaTipo",
                schema: "roadmap",
                table: "CriterioTema",
                columns: new[] { "TemaId", "TipoCriterio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntradaBitacora_TemaId",
                schema: "study",
                table: "EntradaBitacora",
                column: "TemaId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradaBitacora_Usuario_Fecha",
                schema: "study",
                table: "EntradaBitacora",
                columns: new[] { "UsuarioId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Fase_UsuarioId",
                schema: "roadmap",
                table: "Fase",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UQ_Fase_Usuario_Orden",
                schema: "roadmap",
                table: "Fase",
                columns: new[] { "UsuarioId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Herramienta_Nombre",
                schema: "study",
                table: "Herramienta",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Laboratorio_UsuarioId",
                schema: "evidence",
                table: "Laboratorio",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_LaboratorioHerramienta_HerramientaId",
                schema: "evidence",
                table: "LaboratorioHerramienta",
                column: "HerramientaId");

            migrationBuilder.CreateIndex(
                name: "IX_LaboratorioTema_TemaId",
                schema: "evidence",
                table: "LaboratorioTema",
                column: "TemaId");

            migrationBuilder.CreateIndex(
                name: "IX_LogSincronizacion_Conector_Fecha",
                schema: "integration",
                table: "LogSincronizacion",
                columns: new[] { "ConectorId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Nota_ArtefactoTecnicoId",
                schema: "evidence",
                table: "Nota",
                column: "ArtefactoTecnicoId",
                filter: "[ArtefactoTecnicoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Nota_LaboratorioId",
                schema: "evidence",
                table: "Nota",
                column: "LaboratorioId",
                filter: "[LaboratorioId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Nota_ProyectoId",
                schema: "evidence",
                table: "Nota",
                column: "ProyectoId",
                filter: "[ProyectoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Nota_TemaId",
                schema: "evidence",
                table: "Nota",
                column: "TemaId",
                filter: "[TemaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Nota_UsuarioId",
                schema: "evidence",
                table: "Nota",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Nota_WriteupId",
                schema: "evidence",
                table: "Nota",
                column: "WriteupId",
                filter: "[WriteupId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Proyecto_UsuarioId",
                schema: "evidence",
                table: "Proyecto",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoHerramienta_HerramientaId",
                schema: "evidence",
                table: "ProyectoHerramienta",
                column: "HerramientaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProyectoTema_TemaId",
                schema: "evidence",
                table: "ProyectoTema",
                column: "TemaId");

            migrationBuilder.CreateIndex(
                name: "IX_Recurso_UsuarioId",
                schema: "resource",
                table: "Recurso",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RecursoTema_TemaId",
                schema: "resource",
                table: "RecursoTema",
                column: "TemaId");

            migrationBuilder.CreateIndex(
                name: "IX_SesionEstudio_TemaId",
                schema: "study",
                table: "SesionEstudio",
                column: "TemaId");

            migrationBuilder.CreateIndex(
                name: "IX_SesionEstudio_Usuario_Fecha",
                schema: "study",
                table: "SesionEstudio",
                columns: new[] { "UsuarioId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_SesionHerramienta_HerramientaId",
                schema: "study",
                table: "SesionHerramienta",
                column: "HerramientaId");

            migrationBuilder.CreateIndex(
                name: "UQ_SnapshotProgreso_Usuario_Fecha",
                schema: "analytics",
                table: "SnapshotProgreso",
                columns: new[] { "UsuarioId", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tema_FaseId",
                schema: "roadmap",
                table: "Tema",
                column: "FaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Tema_TemaPadreId",
                schema: "roadmap",
                table: "Tema",
                column: "TemaPadreId");

            migrationBuilder.CreateIndex(
                name: "IX_Tema_UsuarioId",
                schema: "roadmap",
                table: "Tema",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TemaDependencia_TemaRequisitoId",
                schema: "roadmap",
                table: "TemaDependencia",
                column: "TemaRequisitoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_CertificacionObjetivoActivaId",
                schema: "nucleo",
                table: "Usuario",
                column: "CertificacionObjetivoActivaId");

            migrationBuilder.CreateIndex(
                name: "UQ_Usuario_Email",
                schema: "nucleo",
                table: "Usuario",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Writeup_UsuarioId",
                schema: "evidence",
                table: "Writeup",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_WriteupTema_TemaId",
                schema: "evidence",
                table: "WriteupTema",
                column: "TemaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtefactoHerramienta",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "ArtefactoTema",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "CertificacionObtenida",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "CertificacionTema",
                schema: "roadmap");

            migrationBuilder.DropTable(
                name: "CompetenciaTema",
                schema: "roadmap");

            migrationBuilder.DropTable(
                name: "CriterioTema",
                schema: "roadmap");

            migrationBuilder.DropTable(
                name: "EntradaBitacora",
                schema: "study");

            migrationBuilder.DropTable(
                name: "LaboratorioHerramienta",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "LaboratorioTema",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "LogSincronizacion",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "Nota",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "ProyectoHerramienta",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "ProyectoTema",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "RecursoTema",
                schema: "resource");

            migrationBuilder.DropTable(
                name: "SesionHerramienta",
                schema: "study");

            migrationBuilder.DropTable(
                name: "SnapshotProgreso",
                schema: "analytics");

            migrationBuilder.DropTable(
                name: "TemaDependencia",
                schema: "roadmap");

            migrationBuilder.DropTable(
                name: "WriteupTema",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "Competencia",
                schema: "roadmap");

            migrationBuilder.DropTable(
                name: "Conector",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "ArtefactoTecnico",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "Laboratorio",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "Proyecto",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "Recurso",
                schema: "resource");

            migrationBuilder.DropTable(
                name: "Herramienta",
                schema: "study");

            migrationBuilder.DropTable(
                name: "SesionEstudio",
                schema: "study");

            migrationBuilder.DropTable(
                name: "Writeup",
                schema: "evidence");

            migrationBuilder.DropTable(
                name: "Tema",
                schema: "roadmap");

            migrationBuilder.DropTable(
                name: "Fase",
                schema: "roadmap");

            migrationBuilder.DropTable(
                name: "Usuario",
                schema: "nucleo");

            migrationBuilder.DropTable(
                name: "Certificacion",
                schema: "roadmap");
        }
    }
}
