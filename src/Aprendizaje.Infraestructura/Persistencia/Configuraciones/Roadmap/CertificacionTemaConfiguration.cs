using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Roadmap;

internal sealed class CertificacionTemaConfiguration : IEntityTypeConfiguration<CertificacionTema>
{
    public void Configure(EntityTypeBuilder<CertificacionTema> builder)
    {
        builder.ToTable("CertificacionTema", "roadmap");

        builder.HasKey(c => new { c.CertificacionId, c.TemaId });

        builder.Property(c => c.CertificacionId).ValueGeneratedNever();
        builder.Property(c => c.TemaId).ValueGeneratedNever();
        builder.Property(c => c.Peso).HasColumnType("decimal(5,2)");

        builder.HasOne<Certificacion>()
            .WithMany()
            .HasForeignKey(c => c.CertificacionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Tema>()
            .WithMany()
            .HasForeignKey(c => c.TemaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(c => c.TemaId).HasDatabaseName("IX_CertificacionTema_TemaId");
    }
}
