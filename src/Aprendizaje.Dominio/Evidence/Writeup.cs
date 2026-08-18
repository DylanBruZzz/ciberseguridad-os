using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Evidence;

/// <summary>
/// Documento técnico de un reto/CTF resuelto. Sin VersionFila: no está en la lista de la
/// convención 11. Sin Domain Event ni Value Objects: mismos criterios ya aplicados a
/// Proyecto y Laboratorio, sin ningún candidato nuevo en este bloque.
/// </summary>
public sealed class Writeup : AggregateRoot, IEliminableLogicamente
{
    public Guid UsuarioId { get; private set; }
    public string Titulo { get; private set; } = null!;
    public string? PlataformaOrigen { get; private set; }
    public string? Url { get; private set; }
    public EstadoMadurez EstadoMadurez { get; private set; }
    public DateOnly? Fecha { get; private set; }

    public DateTime? FechaEliminacionUtc { get; private set; }

    private Writeup() { } // requerido por EF Core

    private Writeup(Guid id, Guid usuarioId, string titulo) : base(id)
    {
        UsuarioId = usuarioId;
        Titulo = titulo;
        EstadoMadurez = EstadoMadurez.Borrador;
    }

    public static Writeup Crear(Guid usuarioId, string titulo)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El Writeup debe pertenecer a un usuario válido.", nameof(usuarioId));

        if (string.IsNullOrWhiteSpace(titulo))
            throw new ArgumentException("El título del Writeup no puede estar vacío.", nameof(titulo));

        return new Writeup(Guid.CreateVersion7(), usuarioId, titulo.Trim());
    }

    public void CambiarTitulo(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ArgumentException("El título del Writeup no puede estar vacío.", nameof(titulo));

        Titulo = titulo.Trim();
    }

    public void ActualizarPlataformaOrigen(string? plataformaOrigen) => PlataformaOrigen = plataformaOrigen;

    public void ActualizarUrl(string? url) => Url = url;

    public void RegistrarFecha(DateOnly fecha) => Fecha = fecha;

    public void AvanzarMadurez(EstadoMadurez estadoMadurez) => EstadoMadurez = estadoMadurez;

    public void MarcarComoEliminado() => FechaEliminacionUtc = DateTime.UtcNow;
}
