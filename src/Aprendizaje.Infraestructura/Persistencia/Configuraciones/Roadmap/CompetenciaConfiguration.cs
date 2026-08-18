using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Roadmap;

public sealed class CompetenciaConfiguration : IEntityTypeConfiguration<Competencia>
{
    public void Configure(EntityTypeBuilder<Competencia> builder)
    {
        builder.ToTable("Competencia", "roadmap");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(c => c.UsuarioId).IsRequired();
        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Descripcion).HasMaxLength(1000);

        builder.Property(c => c.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(c => c.FechaModificacionUtc);
        // Sin FechaEliminacionUtc: Competencia no implementa IEliminableLogicamente.

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(c => new { c.UsuarioId, c.Nombre })
            .IsUnique()
            .HasDatabaseName("UQ_Competencia_Usuario_Nombre");
    }
}
