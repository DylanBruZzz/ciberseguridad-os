using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Roadmap;

public sealed class ApunteTemaConfiguration : IEntityTypeConfiguration<ApunteTema>
{
    public void Configure(EntityTypeBuilder<ApunteTema> builder)
    {
        builder.ToTable("ApunteTema", "roadmap");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(a => a.UsuarioId).IsRequired();
        builder.Property(a => a.TemaId).IsRequired();
        builder.Property(a => a.Contenido).IsRequired().HasColumnType("nvarchar(max)");

        builder.Property(a => a.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(a => a.FechaModificacionUtc);

        // Sin RowVersion ni soft delete: V1 solo requiere reemplazar el texto vivo de apuntes.
        builder.HasOne<Tema>()
            .WithMany()
            .HasForeignKey(a => a.TemaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(a => a.TemaId)
            .IsUnique()
            .HasDatabaseName("IX_ApunteTema_TemaId");
        builder.HasIndex(a => a.UsuarioId).HasDatabaseName("IX_ApunteTema_UsuarioId");
    }
}
