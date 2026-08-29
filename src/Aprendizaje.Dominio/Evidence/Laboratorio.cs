using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Evidence;

/// <summary>
/// Actividad práctica ejecutada. Sin VersionFila: no está en la lista de la convención 11
/// (solo Tema, SesionEstudio y Proyecto la requieren). Sin Domain Event ni Value Objects:
/// mismos criterios ya aplicados a Proyecto, sin ningún candidato nuevo en este bloque.
/// </summary>
public sealed class Laboratorio : AggregateRoot, IEliminableLogicamente
{
    public Guid UsuarioId { get; private set; }
    public string Nombre { get; private set; } = null!;
    public string? Objetivo { get; private set; }
    public string? EntornoVms { get; private set; }
    public string? Hallazgos { get; private set; }
    public int? TiempoInvertidoMinutos { get; private set; }
    public EstadoMadurez EstadoMadurez { get; private set; }
    public DateOnly? Fecha { get; private set; }

    public DateTime? FechaEliminacionUtc { get; private set; }

    private Laboratorio() { } // requerido por EF Core

    private Laboratorio(Guid id, Guid usuarioId, string nombre) : base(id)
    {
        UsuarioId = usuarioId;
        Nombre = nombre;
        EstadoMadurez = EstadoMadurez.Borrador;
    }

    public static Laboratorio Crear(Guid usuarioId, string nombre)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El Laboratorio debe pertenecer a un usuario válido.", nameof(usuarioId));

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del Laboratorio no puede estar vacío.", nameof(nombre));

        return new Laboratorio(Guid.CreateVersion7(), usuarioId, nombre.Trim());
    }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del Laboratorio no puede estar vacío.", nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void ActualizarObjetivo(string? objetivo) => Objetivo = objetivo;

    public void ActualizarEntornoVms(string? entornoVms) => EntornoVms = entornoVms;

    public void RegistrarHallazgos(string? hallazgos) => Hallazgos = hallazgos;

    public void ActualizarTiempoInvertido(int? tiempoInvertidoMinutos) => TiempoInvertidoMinutos = tiempoInvertidoMinutos;

    public void RegistrarFecha(DateOnly fecha) => Fecha = fecha;

    public void ActualizarFecha(DateOnly? fecha) => Fecha = fecha;

    public void AvanzarMadurez(EstadoMadurez estadoMadurez) => EstadoMadurez = estadoMadurez;

    public void MarcarComoEliminado() => FechaEliminacionUtc = DateTime.UtcNow;
}
