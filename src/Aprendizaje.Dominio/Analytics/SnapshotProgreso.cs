using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Analytics;

/// <summary>
/// Snapshot periódico de progreso, calculado y persistido para preservar historial frente a
/// futuros cambios en la fórmula de cálculo. Sin borrado lógico: no implementa
/// IEliminableLogicamente (dato operativo/derivado, excluido explícitamente en la convención 5).
/// Sin VersionFila, sin Domain Event, sin Value Objects: ningún campo tiene invariante ni CHECK
/// en el SQL. Sin métodos de mutación: un snapshot es, por definición, un dato congelado en el
/// momento en que se generó.
/// </summary>
public sealed class SnapshotProgreso : AggregateRoot
{
    public Guid UsuarioId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public decimal PorcentajeGlobal { get; private set; }
    public decimal HorasTotales { get; private set; }
    public int TemasDominados { get; private set; }

    private SnapshotProgreso() { } // requerido por EF Core

    private SnapshotProgreso(
        Guid id, Guid usuarioId, DateOnly fecha, decimal porcentajeGlobal, decimal horasTotales, int temasDominados)
        : base(id)
    {
        UsuarioId = usuarioId;
        Fecha = fecha;
        PorcentajeGlobal = porcentajeGlobal;
        HorasTotales = horasTotales;
        TemasDominados = temasDominados;
    }

    public static SnapshotProgreso Generar(
        Guid usuarioId, DateOnly fecha, decimal porcentajeGlobal, decimal horasTotales, int temasDominados)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El Snapshot de progreso debe pertenecer a un usuario válido.", nameof(usuarioId));

        return new SnapshotProgreso(Guid.CreateVersion7(), usuarioId, fecha, porcentajeGlobal, horasTotales, temasDominados);
    }
}
