using Aprendizaje.Dominio.Comun;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Aprendizaje.Infraestructura.Persistencia.Interceptores;

/// <summary>
/// Gestiona la trazabilidad universal (convención 4): FechaCreacionUtc al insertar,
/// FechaModificacionUtc al modificar, para toda entidad que herede de Entidad — incluidas las
/// entidades internas (CriterioTema, LogSincronizacion), que también heredan de Entidad aunque
/// no tengan DbSet propio, porque se guardan igualmente a través del ChangeTracker cuando su
/// Aggregate Root dueño se persiste.
///
/// Actúa en SavingChanges/SavingChangesAsync (antes de ejecutar el comando), no después —
/// las fechas deben quedar incluidas en el mismo INSERT/UPDATE, a diferencia del despacho de
/// eventos de dominio, que sí ocurre después de confirmar la transacción.
///
/// Escribe las propiedades vía EntityEntry.Property(...), no por asignación directa de C# —
/// FechaCreacionUtc y FechaModificacionUtc tienen setter 'protected' en Entidad; el acceso por
/// metadata de EF Core no está sujeto a esa restricción de accesibilidad (ver comentario en
/// Entidad.cs).
/// </summary>
public sealed class AuditoriaInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        AplicarAuditoria(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AplicarAuditoria(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void AplicarAuditoria(DbContext? contexto)
    {
        if (contexto is null)
            return;

        var ahoraUtc = DateTime.UtcNow;

        foreach (var entry in contexto.ChangeTracker.Entries<Entidad>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(Entidad.FechaCreacionUtc)).CurrentValue = ahoraUtc;
                    break;

                case EntityState.Modified:
                    entry.Property(nameof(Entidad.FechaModificacionUtc)).CurrentValue = ahoraUtc;
                    break;
            }
        }
    }
}
