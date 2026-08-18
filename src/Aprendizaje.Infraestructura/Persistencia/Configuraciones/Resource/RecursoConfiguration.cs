using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Resource;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Resource;

public sealed class RecursoConfiguration : IEntityTypeConfiguration<Recurso>
{
    public void Configure(EntityTypeBuilder<Recurso> builder)
    {
        builder.ToTable("Recurso", "resource", tb =>
        {
            tb.HasCheckConstraint("CK_Recurso_Tipo",
                "[Tipo] IN ('Documentacion','Libro','Curso','Video','Laboratorio','Writeup','Cheatsheet','Script','RepositorioGitHub','NotebookIA','Otro')");

            tb.HasCheckConstraint("CK_Recurso_Estado",
                "[Estado] IN ('PorClasificar','PorRevisar','EnUso','Consultado','Referencia')");

            tb.HasCheckConstraint("CK_Recurso_Rating",
                "[Rating] IS NULL OR [Rating] BETWEEN 1 AND 5");
        });

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(r => r.UsuarioId).IsRequired();

        builder.Property(r => r.Tipo)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>(); // enum como string, convención 9

        builder.Property(r => r.Titulo).IsRequired().HasMaxLength(300);
        builder.Property(r => r.Url).HasMaxLength(500);

        builder.Property(r => r.Estado)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValueSql("N'PorClasificar'")
            .ValueGeneratedNever(); // mismo patrón que Proyecto/Laboratorio/Writeup/ArtefactoTecnico

        // Value Object propio del módulo — Owned Type mapeado en la misma tabla (convención 17)
        builder.OwnsOne(r => r.Rating, rt =>
        {
            rt.Property(v => v.Valor)
                .HasColumnName("Rating")
                .HasColumnType("tinyint");
        });

        builder.Property(r => r.Notas).HasMaxLength(2000);
        builder.Property(r => r.HerramientaIA).HasMaxLength(100);
        builder.Property(r => r.PromptsUtilizados); // NVARCHAR(MAX), sin HasMaxLength por convención EF Core

        builder.Property(r => r.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(r => r.FechaModificacionUtc);
        builder.Property(r => r.FechaEliminacionUtc); // Recurso sí implementa IEliminableLogicamente

        // Sin VersionFila: Recurso no está en la lista de la convención 11.

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        // RecursoTema es tabla de unión sin clase de dominio, fuera del alcance de esta entrega.

        builder.HasQueryFilter(r => r.FechaEliminacionUtc == null); // borrado lógico, convención 5
    }
}
