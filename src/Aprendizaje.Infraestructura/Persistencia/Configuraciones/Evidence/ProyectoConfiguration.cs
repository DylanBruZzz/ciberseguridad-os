using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Evidence;

public sealed class ProyectoConfiguration : IEntityTypeConfiguration<Proyecto>
{
    public void Configure(EntityTypeBuilder<Proyecto> builder)
    {
        builder.ToTable("Proyecto", "evidence", tb =>
        {
            tb.HasCheckConstraint("CK_Proyecto_Estado",
                "[Estado] IN ('Idea','Desarrollo','Documentado','Publicado')");

            tb.HasCheckConstraint("CK_Proyecto_EstadoMadurez",
                "[EstadoMadurez] IN ('Borrador','Documentado','ListoPortafolio','Publicado')");
        });

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(p => p.UsuarioId).IsRequired();
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Descripcion);

        builder.Property(p => p.Estado)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValueSql("N'Idea'")
            .ValueGeneratedNever(); // red de seguridad solo para inserciones externas al dominio

        builder.Property(p => p.EstadoMadurez)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValueSql("N'Borrador'")
            .ValueGeneratedNever();

        builder.Property(p => p.RepositorioUrl).HasMaxLength(500);

        builder.Property(p => p.FechaInicio).HasColumnType("date"); // DateOnly, convención 8
        builder.Property(p => p.FechaFin).HasColumnType("date");

        builder.Property(p => p.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(p => p.FechaModificacionUtc);
        builder.Property(p => p.FechaEliminacionUtc); // Proyecto sí implementa IEliminableLogicamente

        builder.Property(p => p.VersionFila)
            .HasColumnName("RowVersion")
            .IsRowVersion(); // convención 11

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        // ProyectoTema y ProyectoHerramienta son tablas de unión sin clase de dominio;
        // su configuración de skip navigation queda fuera del alcance de esta entrega.

        builder.HasQueryFilter(p => p.FechaEliminacionUtc == null); // borrado lógico, convención 5
    }
}
