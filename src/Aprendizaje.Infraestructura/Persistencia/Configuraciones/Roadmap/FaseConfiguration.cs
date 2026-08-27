using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Roadmap;

public sealed class FaseConfiguration : IEntityTypeConfiguration<Fase>
{
    public void Configure(EntityTypeBuilder<Fase> builder)
    {
        builder.ToTable("Fase", "roadmap", tb =>
        {
            tb.HasCheckConstraint("CK_Fase_MesInicioRecomendado",
                "[MesInicioRecomendado] IS NULL OR [MesInicioRecomendado] >= 1");

            tb.HasCheckConstraint("CK_Fase_MesFinRecomendado",
                "[MesFinRecomendado] IS NULL OR [MesFinRecomendado] >= 1");

            tb.HasCheckConstraint("CK_Fase_MesesRecomendados",
                "[MesInicioRecomendado] IS NULL OR [MesFinRecomendado] IS NULL OR [MesFinRecomendado] >= [MesInicioRecomendado]");
        });

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(f => f.UsuarioId).IsRequired();

        builder.Property(f => f.Nombre).IsRequired().HasMaxLength(150);
        builder.Property(f => f.Orden).IsRequired();
        builder.Property(f => f.Color).HasMaxLength(20);
        builder.Property(f => f.Descripcion).HasMaxLength(1000);
        builder.Property(f => f.MesInicioRecomendado);
        builder.Property(f => f.MesFinRecomendado);
        builder.Property(f => f.CargaSemanalRecomendada).HasMaxLength(50);

        builder.Property<List<string>>("_objetivos")
            .HasColumnName("Objetivos")
            .IsRequired(false)
            .HasConversion(
                objetivos => objetivos.Count == 0 ? null : string.Join('\n', objetivos),
                texto => string.IsNullOrWhiteSpace(texto)
                    ? new List<string>()
                    : texto!.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (a, b) => a!.SequenceEqual(b!),
                a => a!.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                a => a!.ToList()));

        builder.Property<List<string>>("_criteriosAvance")
            .HasColumnName("CriteriosAvance")
            .IsRequired(false)
            .HasConversion(
                criterios => criterios.Count == 0 ? null : string.Join('\n', criterios),
                texto => string.IsNullOrWhiteSpace(texto)
                    ? new List<string>()
                    : texto!.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (a, b) => a!.SequenceEqual(b!),
                a => a!.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                a => a!.ToList()));

        builder.Property(f => f.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()"); // red de seguridad solo para inserciones externas al dominio
        builder.Property(f => f.FechaModificacionUtc);
        // Sin FechaEliminacionUtc: Fase no implementa IEliminableLogicamente — es metadato
        // organizativo, no historia de aprendizaje que proteger con borrado lógico.

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(f => f.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(f => f.UsuarioId).HasDatabaseName("IX_Fase_UsuarioId");
        builder.HasIndex(f => new { f.UsuarioId, f.Orden })
            .IsUnique()
            .HasDatabaseName("UQ_Fase_Usuario_Orden");
    }
}
