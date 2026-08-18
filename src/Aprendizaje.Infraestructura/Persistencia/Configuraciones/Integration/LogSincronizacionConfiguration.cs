using Aprendizaje.Dominio.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Integration;

public sealed class LogSincronizacionConfiguration : IEntityTypeConfiguration<LogSincronizacion>
{
    public void Configure(EntityTypeBuilder<LogSincronizacion> builder)
    {
        builder.ToTable("LogSincronizacion", "integration", tb =>
        {
            tb.HasCheckConstraint("CK_LogSincronizacion_Resultado",
                "[Resultado] IN ('Exito','Error','Parcial')");
        });

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(l => l.ConectorId).IsRequired();

        builder.Property(l => l.Fecha)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(l => l.Resultado)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>(); // enum como string, convención 9

        builder.Property(l => l.Resumen).HasMaxLength(1000);

        builder.Property(l => l.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(l => l.FechaModificacionUtc);
        // Sin FechaEliminacionUtc: entidad interna, sin borrado lógico propio.

        builder.HasIndex(l => new { l.ConectorId, l.Fecha })
            .HasDatabaseName("IX_LogSincronizacion_Conector_Fecha");
    }
}
