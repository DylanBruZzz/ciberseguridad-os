using Aprendizaje.Dominio.Integration;
using Aprendizaje.Dominio.Nucleo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aprendizaje.Infraestructura.Persistencia.Configuraciones.Integration;

public sealed class ConectorConfiguration : IEntityTypeConfiguration<Conector>
{
    public void Configure(EntityTypeBuilder<Conector> builder)
    {
        builder.ToTable("Conector", "integration", tb =>
        {
            tb.HasCheckConstraint("CK_Conector_Plataforma",
                "[Plataforma] IN ('GitHub','TryHackMe','HackTheBox','NotebookLM','Otro')");

            tb.HasCheckConstraint("CK_Conector_Estado",
                "[EstadoConexion] IN ('Conectado','Desconectado','Error')");
        });

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever(); // Guid v7 generado en el dominio, regla global sin DEFAULT en SQL

        builder.Property(c => c.UsuarioId).IsRequired();

        builder.Property(c => c.Plataforma)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>(); // enum como string, convención 9

        builder.Property(c => c.EstadoConexion)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValueSql("N'Desconectado'")
            .ValueGeneratedNever();

        builder.Property(c => c.UltimaSincronizacion);
        builder.Property(c => c.CredencialRef).HasMaxLength(200);

        builder.Property(c => c.FechaCreacionUtc)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(c => c.FechaModificacionUtc);
        // Sin FechaEliminacionUtc: Conector no implementa IEliminableLogicamente.

        // Sin VersionFila: no está en la lista de la convención 11.

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(c => new { c.UsuarioId, c.Plataforma })
            .IsUnique()
            .HasDatabaseName("UQ_Conector_Usuario_Plataforma");

        // LogSincronizacion: entidad interna del agregado — mismo patrón que Tema/CriterioTema.
        builder.HasMany(c => c.Logs)
            .WithOne()
            .HasForeignKey(l => l.ConectorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Logs)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_logs");
    }
}
