using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Comun;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Aprendizaje.Infraestructura.Persistencia.Interceptores;

/// <summary>
/// Despacha los Domain Events acumulados en memoria por cada AggregateRoot, después de que
/// SaveChangesAsync confirme la transacción — nunca antes ni dentro (convención 16.2): publicar
/// antes arriesga procesar un evento cuyo cambio de origen termina revirtiéndose; publicar
/// dentro acopla la duración de la escritura principal a la de sus efectos secundarios.
///
/// El manejo de errores de cada handler (capturar y registrar sin propagar) es responsabilidad
/// de la implementación concreta de IDespachadorEventos, no de este interceptor (convención 16.3).
/// </summary>
public sealed class DespachoEventosInterceptor : SaveChangesInterceptor
{
    private readonly IDespachadorEventos _despachador;

    public DespachoEventosInterceptor(IDespachadorEventos despachador)
    {
        _despachador = despachador;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        var contexto = eventData.Context;
        if (contexto is null)
            return result;

        var raices = contexto.ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.EventosDominio.Any())
            .ToList();

        var eventos = raices.SelectMany(a => a.EventosDominio).ToList();
        raices.ForEach(a => a.LimpiarEventos());

        foreach (var evento in eventos)
            await _despachador.PublicarAsync(evento, cancellationToken);

        return result;
    }
}
