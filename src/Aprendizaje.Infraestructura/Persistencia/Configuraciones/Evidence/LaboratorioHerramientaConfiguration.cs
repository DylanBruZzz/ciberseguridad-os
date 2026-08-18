using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Evidence;

internal sealed class LaboratorioHerramientaConfiguration : IEntityTypeConfiguration<LaboratorioHerramienta>
{
    public void Configure(EntityTypeBuilder<LaboratorioHerramienta> builder)
    {
        builder.ToTable("LaboratorioHerramienta", "evidence");

        builder.HasKey(l => new { l.LaboratorioId, l.HerramientaId });

        builder.Property(l => l.LaboratorioId).ValueGeneratedNever();
        builder.Property(l => l.HerramientaId).ValueGeneratedNever();

        builder.HasOne<Laboratorio>()
            .WithMany()
            .HasForeignKey(l => l.LaboratorioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Herramienta>()
            .WithMany()
            .HasForeignKey(l => l.HerramientaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(l => l.HerramientaId).HasDatabaseName("IX_LaboratorioHerramienta_HerramientaId");
    }
}
