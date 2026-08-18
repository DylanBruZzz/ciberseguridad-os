using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Evidence;

internal sealed class ArtefactoHerramientaConfiguration : IEntityTypeConfiguration<ArtefactoHerramienta>
{
    public void Configure(EntityTypeBuilder<ArtefactoHerramienta> builder)
    {
        builder.ToTable("ArtefactoHerramienta", "evidence");

        builder.HasKey(a => new { a.ArtefactoTecnicoId, a.HerramientaId });

        builder.Property(a => a.ArtefactoTecnicoId).ValueGeneratedNever();
        builder.Property(a => a.HerramientaId).ValueGeneratedNever();

        builder.HasOne<ArtefactoTecnico>()
            .WithMany()
            .HasForeignKey(a => a.ArtefactoTecnicoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Herramienta>()
            .WithMany()
            .HasForeignKey(a => a.HerramientaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(a => a.HerramientaId).HasDatabaseName("IX_ArtefactoHerramienta_HerramientaId");
    }
}
