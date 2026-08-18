using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Evidence;

public sealed class LaboratorioConfiguration : IEntityTypeConfiguration<Laboratorio>
{
    public void Configure(EntityTypeBuilder<Laboratorio> builder)
    {
        builder.ToTable("Laboratorio", "evidence", tb =>
        {
            tb.HasCheckConstraint("CK_Laboratorio_EstadoMadurez",
                "[EstadoMadurez] IN ('Borrador','Documentado','ListoPortafolio','Publicado')");
        });

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(l => l.UsuarioId).IsRequired();
        builder.Property(l => l.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Objetivo).HasMaxLength(1000);
        builder.Property(l => l.EntornoVms).HasMaxLength(1000);
        builder.Property(l => l.Hallazgos); // NVARCHAR(MAX), sin HasMaxLength por convención EF Core

        builder.Property(l => l.TiempoInvertidoMinutos); // sin CHECK en el SQL, sin validación en el dominio

        builder.Property(l => l.EstadoMadurez)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValueSql("N'Borrador'")
            .ValueGeneratedNever(); // mismo patrón ya usado en Proyecto

        builder.Property(l => l.Fecha).HasColumnType("date"); // DateOnly, convención 8

        builder.Property(l => l.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(l => l.FechaModificacionUtc);
        builder.Property(l => l.FechaEliminacionUtc); // Laboratorio sí implementa IEliminableLogicamente

        // Sin VersionFila: Laboratorio no está en la lista de la convención 11.

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(l => l.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        // LaboratorioTema y LaboratorioHerramienta son tablas de unión sin clase de dominio;
        // su configuración queda fuera del alcance de esta entrega, igual que en Proyecto.

        builder.HasQueryFilter(l => l.FechaEliminacionUtc == null); // borrado lógico, convención 5
    }
}
