using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Evidence;

internal sealed class LaboratorioTemaConfiguration : IEntityTypeConfiguration<LaboratorioTema>
{
    public void Configure(EntityTypeBuilder<LaboratorioTema> builder)
    {
        builder.ToTable("LaboratorioTema", "evidence");

        builder.HasKey(l => new { l.LaboratorioId, l.TemaId });

        builder.Property(l => l.LaboratorioId).ValueGeneratedNever();
        builder.Property(l => l.TemaId).ValueGeneratedNever();

        builder.HasOne<Laboratorio>()
            .WithMany()
            .HasForeignKey(l => l.LaboratorioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tema>()
            .WithMany()
            .HasForeignKey(l => l.TemaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(l => l.TemaId).HasDatabaseName("IX_LaboratorioTema_TemaId");
    }
}
