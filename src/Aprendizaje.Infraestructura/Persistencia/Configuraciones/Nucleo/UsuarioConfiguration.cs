using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Nucleo;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario", "nucleo");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(320);

        builder.Property(u => u.FechaRegistro)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(u => u.IntervaloRepasoDefectoDias)
            .IsRequired()
            .HasDefaultValue(30);

        builder.Property(u => u.CertificacionObjetivoActivaId);
        builder.HasOne<Certificacion>()
            .WithMany()
            .HasForeignKey(u => u.CertificacionObjetivoActivaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(u => u.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(u => u.FechaModificacionUtc);
        builder.Property(u => u.FechaEliminacionUtc); // Usuario sí implementa IEliminableLogicamente

        builder.Property(u => u.VersionFila)
            .HasColumnName("RowVersion")
            .IsRowVersion(); // conservado del SQL original — convención 11 revisada

        builder.HasQueryFilter(u => u.FechaEliminacionUtc == null); // borrado lógico, convención 5

        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("UQ_Usuario_Email");
    }
}
