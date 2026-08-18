using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Evidence;

public sealed class CertificacionObtenidaConfiguration : IEntityTypeConfiguration<CertificacionObtenida>
{
    public void Configure(EntityTypeBuilder<CertificacionObtenida> builder)
    {
        builder.ToTable("CertificacionObtenida", "evidence", tb =>
        {
            tb.HasCheckConstraint("CK_CertificacionObtenida_EstadoMadurez",
                "[EstadoMadurez] IN ('Borrador','Documentado','ListoPortafolio','Publicado')");
        });

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(c => c.UsuarioId).IsRequired();
        builder.Property(c => c.CertificacionId).IsRequired();

        builder.Property(c => c.FechaObtencion).IsRequired().HasColumnType("date"); // DateOnly, convención 8
        builder.Property(c => c.EvidenciaUrl).HasMaxLength(500);

        builder.Property(c => c.EstadoMadurez)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValueSql("N'Documentado'")
            .ValueGeneratedNever(); // distinto del resto de Evidence: aquí el default es 'Documentado'

        builder.Property(c => c.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(c => c.FechaModificacionUtc);
        builder.Property(c => c.FechaEliminacionUtc); // CertificacionObtenida sí implementa IEliminableLogicamente

        // Sin VersionFila: CertificacionObtenida no está en la lista de la convención 11.

        // FK_CertificacionObtenida_Certificacion: Evidence → Roadmap es dirección permitida,
        // y Certificacion ya existe como Aggregate Root — se completa aquí.
        builder.HasOne<Certificacion>()
            .WithMany()
            .HasForeignKey(c => c.CertificacionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(c => c.FechaEliminacionUtc == null); // borrado lógico, convención 5

        builder.HasIndex(c => c.UsuarioId).HasDatabaseName("IX_CertificacionObtenida_UsuarioId");
    }
}
