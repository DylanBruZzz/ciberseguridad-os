using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Evidence;

internal sealed class ProyectoHerramientaConfiguration : IEntityTypeConfiguration<ProyectoHerramienta>
{
    public void Configure(EntityTypeBuilder<ProyectoHerramienta> builder)
    {
        builder.ToTable("ProyectoHerramienta", "evidence");

        builder.HasKey(p => new { p.ProyectoId, p.HerramientaId });

        builder.Property(p => p.ProyectoId).ValueGeneratedNever();
        builder.Property(p => p.HerramientaId).ValueGeneratedNever();

        builder.HasOne<Proyecto>()
            .WithMany()
            .HasForeignKey(p => p.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Herramienta>()
            .WithMany()
            .HasForeignKey(p => p.HerramientaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(p => p.HerramientaId).HasDatabaseName("IX_ProyectoHerramienta_HerramientaId");
    }
}
