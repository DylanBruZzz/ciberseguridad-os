using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Study;

public sealed class SesionEstudioConfiguration : IEntityTypeConfiguration<SesionEstudio>
{
    public void Configure(EntityTypeBuilder<SesionEstudio> builder)
    {
        builder.ToTable("SesionEstudio", "study", tb =>
        {
            tb.HasCheckConstraint("CK_SesionEstudio_Duracion", "[DuracionMinutos] > 0");
            tb.HasCheckConstraint("CK_SesionEstudio_Tipo",
                "[Tipo] IN ('Teoria','Practica','Laboratorio','Repaso')");
        });

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(s => s.UsuarioId).IsRequired();
        builder.Property(s => s.TemaId).IsRequired();

        builder.Property(s => s.Fecha).IsRequired().HasColumnType("date"); // DateOnly, convención 8
        builder.Property(s => s.DuracionMinutos).IsRequired();

        builder.Property(s => s.Tipo)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>(); // enum como string, convención 9

        builder.Property(s => s.Notas).HasMaxLength(2000);

        builder.Property(s => s.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(s => s.FechaModificacionUtc);
        builder.Property(s => s.FechaEliminacionUtc); // SesionEstudio sí implementa IEliminableLogicamente

        builder.Property(s => s.VersionFila)
            .HasColumnName("RowVersion")
            .IsRowVersion(); // convención 11

        // FK_SesionEstudio_Tema: Roadmap → Study es dirección permitida en el grafo de módulos,
        // y Tema ya existe como Aggregate Root.
        builder.HasOne<Tema>()
            .WithMany()
            .HasForeignKey(s => s.TemaId)
            .OnDelete(DeleteBehavior.NoAction); // no se puede eliminar un Tema con historial de estudio

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(s => s.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(s => s.FechaEliminacionUtc == null); // borrado lógico, convención 5

        builder.HasIndex(s => s.TemaId).HasDatabaseName("IX_SesionEstudio_TemaId");
        builder.HasIndex(s => new { s.UsuarioId, s.Fecha }).HasDatabaseName("IX_SesionEstudio_Usuario_Fecha");
    }
}
