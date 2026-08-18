using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Evidence;

public sealed class WriteupConfiguration : IEntityTypeConfiguration<Writeup>
{
    public void Configure(EntityTypeBuilder<Writeup> builder)
    {
        builder.ToTable("Writeup", "evidence", tb =>
        {
            tb.HasCheckConstraint("CK_Writeup_EstadoMadurez",
                "[EstadoMadurez] IN ('Borrador','Documentado','ListoPortafolio','Publicado')");
        });

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(w => w.UsuarioId).IsRequired();
        builder.Property(w => w.Titulo).IsRequired().HasMaxLength(200);
        builder.Property(w => w.PlataformaOrigen).HasMaxLength(100);
        builder.Property(w => w.Url).HasMaxLength(500);

        builder.Property(w => w.EstadoMadurez)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValueSql("N'Borrador'")
            .ValueGeneratedNever(); // mismo patrón ya usado en Proyecto y Laboratorio

        builder.Property(w => w.Fecha).HasColumnType("date"); // DateOnly, convención 8

        builder.Property(w => w.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(w => w.FechaModificacionUtc);
        builder.Property(w => w.FechaEliminacionUtc); // Writeup sí implementa IEliminableLogicamente

        // Sin VersionFila: Writeup no está en la lista de la convención 11.

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(w => w.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        // WriteupTema es tabla de unión sin clase de dominio, fuera del alcance de esta entrega.

        builder.HasQueryFilter(w => w.FechaEliminacionUtc == null); // borrado lógico, convención 5
    }
}
