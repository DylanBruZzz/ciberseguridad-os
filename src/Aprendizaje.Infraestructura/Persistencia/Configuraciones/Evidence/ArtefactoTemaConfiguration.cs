using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Evidence;

internal sealed class ArtefactoTemaConfiguration : IEntityTypeConfiguration<ArtefactoTema>
{
    public void Configure(EntityTypeBuilder<ArtefactoTema> builder)
    {
        builder.ToTable("ArtefactoTema", "evidence");

        builder.HasKey(a => new { a.ArtefactoTecnicoId, a.TemaId });

        builder.Property(a => a.ArtefactoTecnicoId).ValueGeneratedNever();
        builder.Property(a => a.TemaId).ValueGeneratedNever();

        builder.HasOne<ArtefactoTecnico>()
            .WithMany()
            .HasForeignKey(a => a.ArtefactoTecnicoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tema>()
            .WithMany()
            .HasForeignKey(a => a.TemaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(a => a.TemaId).HasDatabaseName("IX_ArtefactoTema_TemaId");
    }
}
