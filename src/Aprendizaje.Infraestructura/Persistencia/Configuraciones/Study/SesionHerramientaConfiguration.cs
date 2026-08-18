using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Study;

internal sealed class SesionHerramientaConfiguration : IEntityTypeConfiguration<SesionHerramienta>
{
    public void Configure(EntityTypeBuilder<SesionHerramienta> builder)
    {
        builder.ToTable("SesionHerramienta", "study");

        builder.HasKey(s => new { s.SesionId, s.HerramientaId });

        builder.Property(s => s.SesionId).ValueGeneratedNever();
        builder.Property(s => s.HerramientaId).ValueGeneratedNever();

        builder.HasOne<SesionEstudio>()
            .WithMany()
            .HasForeignKey(s => s.SesionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Herramienta>()
            .WithMany()
            .HasForeignKey(s => s.HerramientaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(s => s.HerramientaId).HasDatabaseName("IX_SesionHerramienta_HerramientaId");
    }
}
