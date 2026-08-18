using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Integration;

/// <summary>
/// Entidad interna del agregado Conector — mismo patrón que CriterioTema dentro de Tema: sin
/// repositorio propio, se crea únicamente mediante Conector.RegistrarLog(...).
/// </summary>
public sealed class LogSincronizacion : Entidad
{
    public Guid ConectorId { get; private set; }
    public DateTime Fecha { get; private set; }
    public ResultadoSincronizacion Resultado { get; private set; }
    public string? Resumen { get; private set; }

    private LogSincronizacion() { } // requerido por EF Core

    internal LogSincronizacion(Guid conectorId, ResultadoSincronizacion resultado, string? resumen)
        : base(Guid.CreateVersion7())
    {
        ConectorId = conectorId;
        Resultado = resultado;
        Resumen = resumen;
        Fecha = DateTime.UtcNow;
    }
}
