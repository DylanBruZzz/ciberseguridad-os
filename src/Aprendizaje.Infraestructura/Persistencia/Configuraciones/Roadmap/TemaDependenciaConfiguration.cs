using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Roadmap;

internal sealed class TemaDependenciaConfiguration : IEntityTypeConfiguration<TemaDependencia>
{
    public void Configure(EntityTypeBuilder<TemaDependencia> builder)
    {
        builder.ToTable("TemaDependencia", "roadmap", tb =>
        {
            tb.HasCheckConstraint("CK_TemaDependencia_NoAutoDependencia", "[TemaId] <> [TemaRequisitoId]");
        });

        builder.HasKey(t => new { t.TemaId, t.TemaRequisitoId });

        builder.Property(t => t.TemaId).ValueGeneratedNever();
        builder.Property(t => t.TemaRequisitoId).ValueGeneratedNever();

        builder.HasOne<Tema>()
            .WithMany()
            .HasForeignKey(t => t.TemaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Tema>()
            .WithMany()
            .HasForeignKey(t => t.TemaRequisitoId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(t => t.TemaRequisitoId).HasDatabaseName("IX_TemaDependencia_TemaRequisitoId");
    }
}
