using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Evidence;

internal sealed class ProyectoTemaConfiguration : IEntityTypeConfiguration<ProyectoTema>
{
    public void Configure(EntityTypeBuilder<ProyectoTema> builder)
    {
        builder.ToTable("ProyectoTema", "evidence");

        builder.HasKey(p => new { p.ProyectoId, p.TemaId });

        builder.Property(p => p.ProyectoId).ValueGeneratedNever();
        builder.Property(p => p.TemaId).ValueGeneratedNever();

        builder.HasOne<Proyecto>()
            .WithMany()
            .HasForeignKey(p => p.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tema>()
            .WithMany()
            .HasForeignKey(p => p.TemaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(p => p.TemaId).HasDatabaseName("IX_ProyectoTema_TemaId");
    }
}
