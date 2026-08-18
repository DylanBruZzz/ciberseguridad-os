using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Roadmap;

public sealed class TemaConfiguration : IEntityTypeConfiguration<Tema>
{
    public void Configure(EntityTypeBuilder<Tema> builder)
    {
        builder.ToTable("Tema", "roadmap", tb =>
        {
            tb.HasCheckConstraint("CK_Tema_TipoConocimiento",
                "[TipoConocimiento] IN ('Conceptual','Procedimental','Herramienta')");

            tb.HasCheckConstraint("CK_Tema_Dificultad",
                "[DificultadPercibida] IS NULL OR [DificultadPercibida] BETWEEN 1 AND 5");

            tb.HasCheckConstraint("CK_Tema_Confianza",
                "[Confianza] IS NULL OR [Confianza] BETWEEN 1 AND 5");

            tb.HasCheckConstraint("CK_Tema_NoAutoPadre",
                "[TemaPadreId] IS NULL OR [TemaPadreId] <> [Id]");
        });

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, convención 6

        builder.Property(t => t.UsuarioId).IsRequired();
        builder.Property(t => t.FaseId);
        builder.Property(t => t.TemaPadreId);

        builder.Property(t => t.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Descripcion);

        builder.Property(t => t.TipoConocimiento)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>(); // enum como string, convención 9

        builder.Property(t => t.FechaInicio).HasColumnType("date"); // DateOnly, convención 8
        builder.Property(t => t.FechaFin).HasColumnType("date");

        builder.Property(t => t.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()"); // red de seguridad solo para inserciones externas al dominio
        builder.Property(t => t.FechaModificacionUtc);
        builder.Property(t => t.FechaEliminacionUtc); // Tema sí implementa IEliminableLogicamente

        builder.Property(t => t.VersionFila)
            .HasColumnName("RowVersion")
            .IsRowVersion(); // convención 11: Tema es una de las tres entidades con concurrencia optimista

        // Objetivos: propiedad de solo lectura respaldada por campo privado (convención 10) —
        // se configura sobre el campo directamente, convertido a una columna de texto delimitado.
        builder.Property<List<string>>("_objetivos")
            .HasColumnName("Objetivos")
            .HasConversion(
                objetivos => objetivos.Count == 0 ? null : string.Join('\n', objetivos),
                texto => string.IsNullOrWhiteSpace(texto)
                    ? new List<string>()
                    : texto!.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (a, b) => a!.SequenceEqual(b!),
                a => a!.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                a => a!.ToList()));

        // Value Objects como Owned Types, mapeados dentro de la misma tabla (convención 17)
        builder.OwnsOne(t => t.DificultadPercibida, np =>
        {
            np.Property(n => n.Valor)
                .HasColumnName("DificultadPercibida")
                .HasColumnType("tinyint");
        });

        builder.OwnsOne(t => t.Confianza, np =>
        {
            np.Property(n => n.Valor)
                .HasColumnName("Confianza")
                .HasColumnType("tinyint");
        });

        builder.OwnsOne(t => t.IntervaloRepaso, ir =>
        {
            ir.Property(i => i.Dias).HasColumnName("IntervaloRepasoDias");
        });

        // CriterioTema: entidad interna del agregado — colección respaldada por campo privado,
        // sin navegación inversa (CriterioTema no conoce a Tema), CASCADE porque es el único
        // camino de escritura hacia CriterioTema (convención de la revisión de implementación).
        builder.HasMany(t => t.Criterios)
            .WithOne()
            .HasForeignKey(c => c.TemaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Criterios)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_criterios");

        // Autorreferencia TemaPadre: única relación cross-Aggregate-Root configurable hoy,
        // porque Tema ya existe como tipo. NO ACTION, igual que el resto de referencias hacia
        // Tema en el esquema relacional.
        builder.HasOne<Tema>()
            .WithMany()
            .HasForeignKey(t => t.TemaPadreId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Fase>()
            .WithMany()
            .HasForeignKey(t => t.FaseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(t => t.FechaEliminacionUtc == null); // borrado lógico, convención 5

        builder.HasIndex(t => t.UsuarioId).HasDatabaseName("IX_Tema_UsuarioId");
        builder.HasIndex(t => t.FaseId).HasDatabaseName("IX_Tema_FaseId");
        builder.HasIndex(t => t.TemaPadreId).HasDatabaseName("IX_Tema_TemaPadreId");
    }
}
