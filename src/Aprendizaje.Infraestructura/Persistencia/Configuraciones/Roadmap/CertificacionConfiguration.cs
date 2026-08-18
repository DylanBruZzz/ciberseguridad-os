using Aprendizaje.Dominio.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Roadmap;

public sealed class CertificacionConfiguration : IEntityTypeConfiguration<Certificacion>
{
    public void Configure(EntityTypeBuilder<Certificacion> builder)
    {
        builder.ToTable("Certificacion", "roadmap", tb =>
        {
            tb.HasCheckConstraint("CK_Certificacion_TipoCosto", "[TipoCosto] IN ('Gratuita','Pago')");
        });

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Proveedor).HasMaxLength(150);

        builder.Property(c => c.TipoCosto)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>(); // enum como string, convención 9

        builder.Property(c => c.Url).HasMaxLength(500);

        builder.Property(c => c.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(c => c.FechaModificacionUtc);
        // Sin FechaEliminacionUtc: catálogo compartido, no implementa IEliminableLogicamente.

        // Sin FK de Usuario: Certificacion es catálogo global, no pertenece a un usuario
        // (decisión ya congelada en el modelo relacional — evita duplicación entre usuarios).

        builder.HasIndex(c => c.Nombre)
            .IsUnique()
            .HasDatabaseName("UQ_Certificacion_Nombre");
    }
}
