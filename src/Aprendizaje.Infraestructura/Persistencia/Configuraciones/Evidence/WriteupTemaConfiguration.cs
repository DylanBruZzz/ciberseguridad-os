using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Evidence;

internal sealed class WriteupTemaConfiguration : IEntityTypeConfiguration<WriteupTema>
{
    public void Configure(EntityTypeBuilder<WriteupTema> builder)
    {
        builder.ToTable("WriteupTema", "evidence");

        builder.HasKey(w => new { w.WriteupId, w.TemaId });

        builder.Property(w => w.WriteupId).ValueGeneratedNever();
        builder.Property(w => w.TemaId).ValueGeneratedNever();

        builder.HasOne<Writeup>()
            .WithMany()
            .HasForeignKey(w => w.WriteupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tema>()
            .WithMany()
            .HasForeignKey(w => w.TemaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(w => w.TemaId).HasDatabaseName("IX_WriteupTema_TemaId");
    }
}
