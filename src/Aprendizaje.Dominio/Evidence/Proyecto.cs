using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Evidence;

/// <summary>
/// Trabajo desarrollado, con ciclo de vida propio (Estado) y madurez hacia el portafolio
/// (EstadoMadurez) — dos conceptos distintos: Estado es el progreso del trabajo en sí;
/// EstadoMadurez es su preparación para publicarse como evidencia. Sin Domain Event: el
/// Portfolio engine es una capa de lectura pura sobre EstadoMadurez y no reacciona a eventos;
/// la sugerencia de "listo para portafolio" al pasar a Documentado es un caso de uso de
/// Aplicación invocado directamente, no un Domain Event.
/// </summary>
public sealed class Proyecto : AggregateRoot, IEliminableLogicamente
{
    public Guid UsuarioId { get; private set; }
    public string Nombre { get; private set; } = null!;
    public string? Descripcion { get; private set; }
    public EstadoProyecto Estado { get; private set; }
    public EstadoMadurez EstadoMadurez { get; private set; }
    public string? RepositorioUrl { get; private set; }
    public DateOnly? FechaInicio { get; private set; }
    public DateOnly? FechaFin { get; private set; }

    /// <summary>Token de concurrencia optimista — convención 11: una de las tres entidades que lo requieren.</summary>
    public byte[]? VersionFila { get; private set; }

    public DateTime? FechaEliminacionUtc { get; private set; }

    private Proyecto() { } // requerido por EF Core

    private Proyecto(Guid id, Guid usuarioId, string nombre) : base(id)
    {
        UsuarioId = usuarioId;
        Nombre = nombre;
        Estado = EstadoProyecto.Idea;
        EstadoMadurez = EstadoMadurez.Borrador;
    }

    public static Proyecto Crear(Guid usuarioId, string nombre)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El Proyecto debe pertenecer a un usuario válido.", nameof(usuarioId));

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del Proyecto no puede estar vacío.", nameof(nombre));

        return new Proyecto(Guid.CreateVersion7(), usuarioId, nombre.Trim());
    }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del Proyecto no puede estar vacío.", nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void ActualizarDescripcion(string? descripcion) => Descripcion = descripcion;

    public void ActualizarRepositorioUrl(string? url) => RepositorioUrl = url;

    public void AvanzarEstado(EstadoProyecto estado) => Estado = estado;

    public void AvanzarMadurez(EstadoMadurez estadoMadurez) => EstadoMadurez = estadoMadurez;

    public void IniciarDesarrollo(DateOnly fecha)
    {
        if (FechaFin.HasValue && fecha > FechaFin.Value)
            throw new InvalidOperationException("La fecha de inicio no puede ser posterior a la fecha de fin ya registrada.");

        FechaInicio = fecha;
    }

    public void FinalizarDesarrollo(DateOnly fecha)
    {
        if (FechaInicio.HasValue && fecha < FechaInicio.Value)
            throw new InvalidOperationException("La fecha de fin no puede ser anterior a la fecha de inicio.");

        FechaFin = fecha;
    }

    public void MarcarComoEliminado() => FechaEliminacionUtc = DateTime.UtcNow;
}
