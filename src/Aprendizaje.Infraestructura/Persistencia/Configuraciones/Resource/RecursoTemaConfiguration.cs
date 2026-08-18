using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Resource;

internal sealed class RecursoTemaConfiguration : IEntityTypeConfiguration<RecursoTema>
{
    public void Configure(EntityTypeBuilder<RecursoTema> builder)
    {
        builder.ToTable("RecursoTema", "resource");

        builder.HasKey(r => new { r.RecursoId, r.TemaId });

        builder.Property(r => r.RecursoId).ValueGeneratedNever();
        builder.Property(r => r.TemaId).ValueGeneratedNever();

        builder.HasOne<Recurso>()
            .WithMany()
            .HasForeignKey(r => r.RecursoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tema>()
            .WithMany()
            .HasForeignKey(r => r.TemaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(r => r.TemaId).HasDatabaseName("IX_RecursoTema_TemaId");
    }
}
