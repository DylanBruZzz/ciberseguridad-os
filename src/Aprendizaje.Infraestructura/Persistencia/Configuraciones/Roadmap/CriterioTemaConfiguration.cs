using Aprendizaje.Dominio.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Roadmap;

public sealed class CriterioTemaConfiguration : IEntityTypeConfiguration<CriterioTema>
{
    public void Configure(EntityTypeBuilder<CriterioTema> builder)
    {
        builder.ToTable("CriterioTema", "roadmap", tb =>
        {
            tb.HasCheckConstraint("CK_CriterioTema_Tipo",
                "[TipoCriterio] IN ('Teoria','Practica','Explicacion','Ejercicios','Laboratorio')");
        });

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, convención 6

        builder.Property(c => c.TemaId).IsRequired();

        builder.Property(c => c.Tipo)
            .HasColumnName("TipoCriterio")
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>(); // enum como string, convención 9

        builder.Property(c => c.Cumplido)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.FechaCumplido);

        builder.Property(c => c.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()"); // red de seguridad solo para inserciones externas al dominio
        builder.Property(c => c.FechaModificacionUtc);
        // Sin FechaEliminacionUtc: CriterioTema no implementa IEliminableLogicamente —
        // su ciclo de vida está atado al de Tema (CASCADE físico), no tiene historia propia
        // que proteger de forma independiente.

        builder.HasIndex(c => c.TemaId)
            .HasDatabaseName("IX_CriterioTema_TemaId");

        builder.HasIndex(c => new { c.TemaId, c.Tipo })
            .IsUnique()
            .HasDatabaseName("UQ_CriterioTema_TemaTipo");
    }
}
