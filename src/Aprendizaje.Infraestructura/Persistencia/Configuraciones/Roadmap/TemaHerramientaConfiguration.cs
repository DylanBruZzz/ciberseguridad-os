using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Roadmap;

internal sealed class TemaHerramientaConfiguration : IEntityTypeConfiguration<TemaHerramienta>
{
    public void Configure(EntityTypeBuilder<TemaHerramienta> builder)
    {
        builder.ToTable("TemaHerramienta", "roadmap");

        builder.HasKey(t => new { t.TemaId, t.HerramientaId });

        builder.Property(t => t.TemaId).ValueGeneratedNever();
        builder.Property(t => t.HerramientaId).ValueGeneratedNever();

        builder.HasOne<Tema>()
            .WithMany()
            .HasForeignKey(t => t.TemaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Herramienta>()
            .WithMany()
            .HasForeignKey(t => t.HerramientaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(t => t.HerramientaId).HasDatabaseName("IX_TemaHerramienta_HerramientaId");
    }
}
