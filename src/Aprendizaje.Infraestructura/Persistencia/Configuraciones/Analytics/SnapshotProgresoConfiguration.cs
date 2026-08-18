using Aprendizaje.Dominio.Analytics;
using Aprendizaje.Dominio.Nucleo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Analytics;

public sealed class SnapshotProgresoConfiguration : IEntityTypeConfiguration<SnapshotProgreso>
{
    public void Configure(EntityTypeBuilder<SnapshotProgreso> builder)
    {
        builder.ToTable("SnapshotProgreso", "analytics");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(s => s.UsuarioId).IsRequired();
        builder.Property(s => s.Fecha).IsRequired().HasColumnType("date"); // DateOnly, convención 8

        builder.Property(s => s.PorcentajeGlobal).IsRequired().HasColumnType("decimal(5,2)");
        builder.Property(s => s.HorasTotales).IsRequired().HasColumnType("decimal(10,2)");
        builder.Property(s => s.TemasDominados).IsRequired();

        builder.Property(s => s.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(s => s.FechaModificacionUtc);
        // Sin FechaEliminacionUtc: SnapshotProgreso no implementa IEliminableLogicamente.
        // Sin HasQueryFilter, coherente con lo anterior.

        // Sin VersionFila: no está en la lista de la convención 11.

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(s => s.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(s => new { s.UsuarioId, s.Fecha })
            .IsUnique()
            .HasDatabaseName("UQ_SnapshotProgreso_Usuario_Fecha");
    }
}
