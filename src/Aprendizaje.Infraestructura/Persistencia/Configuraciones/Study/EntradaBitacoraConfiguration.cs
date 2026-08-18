using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Study;

public sealed class EntradaBitacoraConfiguration : IEntityTypeConfiguration<EntradaBitacora>
{
    public void Configure(EntityTypeBuilder<EntradaBitacora> builder)
    {
        builder.ToTable("EntradaBitacora", "study");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(e => e.UsuarioId).IsRequired();
        builder.Property(e => e.TemaId);

        builder.Property(e => e.Fecha)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()"); // Fecha de negocio: cuándo se escribió la entrada

        builder.Property(e => e.Texto).IsRequired();

        builder.Property(e => e.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()"); // Trazabilidad técnica, distinta de Fecha
        builder.Property(e => e.FechaModificacionUtc);
        builder.Property(e => e.FechaEliminacionUtc); // EntradaBitacora sí implementa IEliminableLogicamente

        // Sin VersionFila: EntradaBitacora no está en la lista de la convención 11.

        // FK_EntradaBitacora_Tema: Roadmap → Study es dirección permitida, y Tema ya existe.
        builder.HasOne<Tema>()
            .WithMany()
            .HasForeignKey(e => e.TemaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(e => e.FechaEliminacionUtc == null); // borrado lógico, convención 5

        builder.HasIndex(e => new { e.UsuarioId, e.Fecha }).HasDatabaseName("IX_EntradaBitacora_Usuario_Fecha");
    }
}
