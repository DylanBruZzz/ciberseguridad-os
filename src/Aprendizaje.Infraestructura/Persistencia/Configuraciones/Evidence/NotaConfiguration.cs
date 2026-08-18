using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Evidence;

public sealed class NotaConfiguration : IEntityTypeConfiguration<Nota>
{
    public void Configure(EntityTypeBuilder<Nota> builder)
    {
        builder.ToTable("Nota", "evidence", tb =>
        {
            tb.HasCheckConstraint("CK_Nota_Tipo",
                "[Tipo] IN ('Nota','Hallazgo','Actualizacion','Autoexplicacion')");

            tb.HasCheckConstraint("CK_Nota_UnSoloPadre",
                "(CASE WHEN [TemaId] IS NOT NULL THEN 1 ELSE 0 END) + " +
                "(CASE WHEN [ProyectoId] IS NOT NULL THEN 1 ELSE 0 END) + " +
                "(CASE WHEN [LaboratorioId] IS NOT NULL THEN 1 ELSE 0 END) + " +
                "(CASE WHEN [WriteupId] IS NOT NULL THEN 1 ELSE 0 END) + " +
                "(CASE WHEN [ArtefactoTecnicoId] IS NOT NULL THEN 1 ELSE 0 END) = 1");
        });

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(n => n.UsuarioId).IsRequired();
        builder.Property(n => n.TemaId);
        builder.Property(n => n.ProyectoId);
        builder.Property(n => n.LaboratorioId);
        builder.Property(n => n.WriteupId);
        builder.Property(n => n.ArtefactoTecnicoId);

        builder.Property(n => n.Fecha)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(n => n.Texto).IsRequired();

        builder.Property(n => n.Tipo)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>(); // enum como string, convención 9

        builder.Property(n => n.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(n => n.FechaModificacionUtc);
        builder.Property(n => n.FechaEliminacionUtc); // Nota sí implementa IEliminableLogicamente

        // Sin VersionFila: Nota no está en la lista de la convención 11.

        // Las 5 FKs opcionales se completan aquí: Tema, Proyecto, Laboratorio, Writeup y
        // ArtefactoTecnico ya existen como Aggregate Roots — todas NO ACTION, porque Nota es
        // un Aggregate Root independiente y nunca debe arrastrarse al borrar su "padre".
        builder.HasOne<Tema>().WithMany().HasForeignKey(n => n.TemaId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<Proyecto>().WithMany().HasForeignKey(n => n.ProyectoId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<Laboratorio>().WithMany().HasForeignKey(n => n.LaboratorioId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<Writeup>().WithMany().HasForeignKey(n => n.WriteupId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<ArtefactoTecnico>().WithMany().HasForeignKey(n => n.ArtefactoTecnicoId).OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Usuario>().WithMany().HasForeignKey(n => n.UsuarioId).OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(n => n.FechaEliminacionUtc == null); // borrado lógico, convención 5

        // Índices filtrados: cada uno solo indexa las filas donde ese padre aplica.
        builder.HasIndex(n => n.TemaId).HasFilter("[TemaId] IS NOT NULL").HasDatabaseName("IX_Nota_TemaId");
        builder.HasIndex(n => n.ProyectoId).HasFilter("[ProyectoId] IS NOT NULL").HasDatabaseName("IX_Nota_ProyectoId");
        builder.HasIndex(n => n.LaboratorioId).HasFilter("[LaboratorioId] IS NOT NULL").HasDatabaseName("IX_Nota_LaboratorioId");
        builder.HasIndex(n => n.WriteupId).HasFilter("[WriteupId] IS NOT NULL").HasDatabaseName("IX_Nota_WriteupId");
        builder.HasIndex(n => n.ArtefactoTecnicoId).HasFilter("[ArtefactoTecnicoId] IS NOT NULL").HasDatabaseName("IX_Nota_ArtefactoTecnicoId");
    }
}
