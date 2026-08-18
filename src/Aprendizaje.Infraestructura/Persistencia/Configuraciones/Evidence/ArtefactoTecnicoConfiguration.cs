using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Evidence;

public sealed class ArtefactoTecnicoConfiguration : IEntityTypeConfiguration<ArtefactoTecnico>
{
    public void Configure(EntityTypeBuilder<ArtefactoTecnico> builder)
    {
        builder.ToTable("ArtefactoTecnico", "evidence", tb =>
        {
            tb.HasCheckConstraint("CK_ArtefactoTecnico_Tipo",
                "[TipoArtefacto] IN ('Script','Herramienta','Cheatsheet','Dashboard','Playbook','ReglaDeteccion','ConsultaSiem','Automatizacion','Plantilla','Otro')");

            tb.HasCheckConstraint("CK_ArtefactoTecnico_EstadoMadurez",
                "[EstadoMadurez] IN ('Borrador','Documentado','ListoPortafolio','Publicado')");
        });

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(a => a.UsuarioId).IsRequired();

        builder.Property(a => a.TipoArtefacto)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>(); // enum como string, convención 9

        builder.Property(a => a.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(a => a.ContenidoOUrl); // NVARCHAR(MAX), sin HasMaxLength por convención EF Core
        builder.Property(a => a.LenguajeTecnologia).HasMaxLength(100);

        builder.Property(a => a.EstadoMadurez)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValueSql("N'Borrador'")
            .ValueGeneratedNever(); // mismo patrón ya usado en Proyecto, Laboratorio y Writeup

        builder.Property(a => a.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(a => a.FechaModificacionUtc);
        builder.Property(a => a.FechaEliminacionUtc); // ArtefactoTecnico sí implementa IEliminableLogicamente

        // Sin VersionFila: ArtefactoTecnico no está en la lista de la convención 11.

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        // ArtefactoTema y ArtefactoHerramienta son tablas de unión sin clase de dominio,
        // fuera del alcance de esta entrega.

        builder.HasQueryFilter(a => a.FechaEliminacionUtc == null); // borrado lógico, convención 5
    }
}
