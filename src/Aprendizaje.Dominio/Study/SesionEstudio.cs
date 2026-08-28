using Aprendizaje.Dominio.Comun;
using Aprendizaje.Dominio.Study.Eventos;

namespace Aprendizaje.Dominio.Study;

/// <summary>
/// Registro atómico de tiempo invertido en un Tema. A diferencia de Nota, es editable —
/// no append-only — porque corregir una duración mal registrada es una operación legítima
/// (decisión ya establecida en la validación de casos de uso frecuentes).
/// </summary>
public sealed class SesionEstudio : AggregateRoot, IEliminableLogicamente
{
    public Guid UsuarioId { get; private set; }
    public Guid TemaId { get; private set; }
    public DateOnly Fecha { get; private set; }
    public int DuracionMinutos { get; private set; }
    public TipoSesion Tipo { get; private set; }
    public string? Notas { get; private set; }

    /// <summary>Token de concurrencia optimista — convención 11: una de las tres entidades que lo requieren.</summary>
    public byte[]? VersionFila { get; private set; }

    public DateTime? FechaEliminacionUtc { get; private set; }

    private SesionEstudio() { } // requerido por EF Core

    private SesionEstudio(
        Guid id, Guid usuarioId, Guid temaId, DateOnly fecha, int duracionMinutos, TipoSesion tipo, string? notas)
        : base(id)
    {
        UsuarioId = usuarioId;
        TemaId = temaId;
        Fecha = fecha;
        DuracionMinutos = duracionMinutos;
        Tipo = tipo;
        Notas = notas;
    }

    public static SesionEstudio Registrar(
        Guid usuarioId, Guid temaId, DateOnly fecha, int duracionMinutos, TipoSesion tipo, string? notas = null)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("La Sesión de estudio debe pertenecer a un usuario válido.", nameof(usuarioId));

        if (temaId == Guid.Empty)
            throw new ArgumentException("La Sesión de estudio debe estar vinculada a un Tema válido.", nameof(temaId));

        if (duracionMinutos <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(duracionMinutos), duracionMinutos, "La duración de la sesión debe ser mayor a cero minutos.");

        var sesion = new SesionEstudio(Guid.CreateVersion7(), usuarioId, temaId, fecha, duracionMinutos, tipo, notas);

        sesion.RegistrarEvento(new SesionRegistradaEvento(sesion.Id, temaId, DateTime.UtcNow));

        return sesion;
    }

    public void CorregirDuracion(int duracionMinutos)
    {
        if (duracionMinutos <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(duracionMinutos), duracionMinutos, "La duración de la sesión debe ser mayor a cero minutos.");

        DuracionMinutos = duracionMinutos;
    }

    public void CambiarFecha(DateOnly fecha) => Fecha = fecha;

    public void CambiarTema(Guid temaId)
    {
        if (temaId == Guid.Empty)
            throw new ArgumentException("La Sesión de estudio debe estar vinculada a un Tema válido.", nameof(temaId));

        TemaId = temaId;
    }

    public void CambiarTipo(TipoSesion tipo) => Tipo = tipo;

    public void ActualizarNotas(string? notas) => Notas = notas;

    public void MarcarComoEliminado() => FechaEliminacionUtc = DateTime.UtcNow;
}
