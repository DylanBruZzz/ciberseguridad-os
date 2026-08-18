using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Evidence;

/// <summary>
/// Generalización deliberada de scripts, cheatsheets, dashboards, reglas de detección y demás
/// artefactos técnicos que comparten la misma forma (nombre, contenido, tecnología, madurez) —
/// ver decisión de generalización de la revisión de artefactos técnicos. Sin VersionFila, sin
/// Domain Event ni Value Objects: mismos criterios ya aplicados a Proyecto/Laboratorio/Writeup.
/// </summary>
public sealed class ArtefactoTecnico : AggregateRoot, IEliminableLogicamente
{
    public Guid UsuarioId { get; private set; }
    public TipoArtefacto TipoArtefacto { get; private set; }
    public string Nombre { get; private set; } = null!;
    public string? ContenidoOUrl { get; private set; }
    public string? LenguajeTecnologia { get; private set; }
    public EstadoMadurez EstadoMadurez { get; private set; }

    public DateTime? FechaEliminacionUtc { get; private set; }

    private ArtefactoTecnico() { } // requerido por EF Core

    private ArtefactoTecnico(Guid id, Guid usuarioId, TipoArtefacto tipoArtefacto, string nombre) : base(id)
    {
        UsuarioId = usuarioId;
        TipoArtefacto = tipoArtefacto;
        Nombre = nombre;
        EstadoMadurez = EstadoMadurez.Borrador;
    }

    public static ArtefactoTecnico Crear(Guid usuarioId, TipoArtefacto tipoArtefacto, string nombre)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El Artefacto Técnico debe pertenecer a un usuario válido.", nameof(usuarioId));

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del Artefacto Técnico no puede estar vacío.", nameof(nombre));

        return new ArtefactoTecnico(Guid.CreateVersion7(), usuarioId, tipoArtefacto, nombre.Trim());
    }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del Artefacto Técnico no puede estar vacío.", nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void ActualizarContenidoOUrl(string? contenidoOUrl) => ContenidoOUrl = contenidoOUrl;

    public void ActualizarLenguajeTecnologia(string? lenguajeTecnologia) => LenguajeTecnologia = lenguajeTecnologia;

    public void CambiarTipoArtefacto(TipoArtefacto tipoArtefacto) => TipoArtefacto = tipoArtefacto;

    public void AvanzarMadurez(EstadoMadurez estadoMadurez) => EstadoMadurez = estadoMadurez;

    public void MarcarComoEliminado() => FechaEliminacionUtc = DateTime.UtcNow;
}
