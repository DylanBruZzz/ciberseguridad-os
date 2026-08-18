using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Roadmap;

public sealed class FaseConfiguration : IEntityTypeConfiguration<Fase>
{
    public void Configure(EntityTypeBuilder<Fase> builder)
    {
        builder.ToTable("Fase", "roadmap");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(f => f.UsuarioId).IsRequired();

        builder.Property(f => f.Nombre).IsRequired().HasMaxLength(150);
        builder.Property(f => f.Orden).IsRequired();
        builder.Property(f => f.Color).HasMaxLength(20);
        builder.Property(f => f.Descripcion).HasMaxLength(1000);

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
