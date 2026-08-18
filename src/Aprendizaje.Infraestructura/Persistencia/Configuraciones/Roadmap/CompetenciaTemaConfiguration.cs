using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Roadmap;

internal sealed class CompetenciaTemaConfiguration : IEntityTypeConfiguration<CompetenciaTema>
{
    public void Configure(EntityTypeBuilder<CompetenciaTema> builder)
    {
        builder.ToTable("CompetenciaTema", "roadmap");

        builder.HasKey(c => new { c.CompetenciaId, c.TemaId });

        builder.Property(c => c.CompetenciaId).ValueGeneratedNever();
        builder.Property(c => c.TemaId).ValueGeneratedNever();

        builder.HasOne<Competencia>()
            .WithMany()
            .HasForeignKey(c => c.CompetenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tema>()
            .WithMany()
            .HasForeignKey(c => c.TemaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(c => c.TemaId).HasDatabaseName("IX_CompetenciaTema_TemaId");
    }
}
