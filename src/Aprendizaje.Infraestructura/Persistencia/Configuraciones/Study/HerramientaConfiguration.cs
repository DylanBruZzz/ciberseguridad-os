using Aprendizaje.Dominio.Study;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Study;

public sealed class HerramientaConfiguration : IEntityTypeConfiguration<Herramienta>
{
    public void Configure(EntityTypeBuilder<Herramienta> builder)
    {
        builder.ToTable("Herramienta", "study");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(h => h.Nombre).IsRequired().HasMaxLength(150);
        builder.Property(h => h.Categoria).HasMaxLength(50);

        builder.Property(h => h.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(h => h.FechaModificacionUtc);
        // Sin FechaEliminacionUtc: catálogo compartido, no implementa IEliminableLogicamente.

        // Sin FK de Usuario: Herramienta es catálogo global, no pertenece a un usuario
        // (mismo criterio ya congelado para Certificacion).

        builder.HasIndex(h => h.Nombre)
            .IsUnique()
            .HasDatabaseName("UQ_Herramienta_Nombre");
    }
}
