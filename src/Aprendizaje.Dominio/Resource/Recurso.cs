using Aprendizaje.Dominio.Comun;
using Aprendizaje.Dominio.Resource.ValueObjects;

namespace Aprendizaje.Dominio.Resource;

/// <summary>
/// Biblioteca de consumo. Nace en estado PorClasificar — guardar un recurso es un gesto rápido
/// que no debe exigir clasificación inmediata (decisión de producto ya establecida). Sin
/// VersionFila: Recurso no está en la lista de la convención 11. Sin Domain Event: mismo
/// criterio ya aplicado al resto del dominio, sin consumidor pre-aprobado.
/// </summary>
public sealed class Recurso : AggregateRoot, IEliminableLogicamente
{
    public Guid UsuarioId { get; private set; }
    public TipoRecurso Tipo { get; private set; }
    public string Titulo { get; private set; } = null!;
    public string? Url { get; private set; }
    public EstadoRecurso Estado { get; private set; }
    public RatingRecurso? Rating { get; private set; }
    public string? Notas { get; private set; }
    public string? HerramientaIA { get; private set; }
    public string? PromptsUtilizados { get; private set; }

    public DateTime? FechaEliminacionUtc { get; private set; }

    private Recurso() { } // requerido por EF Core

    private Recurso(Guid id, Guid usuarioId, TipoRecurso tipo, string titulo) : base(id)
    {
        UsuarioId = usuarioId;
        Tipo = tipo;
        Titulo = titulo;
        Estado = EstadoRecurso.PorClasificar;
    }

    public static Recurso Guardar(Guid usuarioId, TipoRecurso tipo, string titulo)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El Recurso debe pertenecer a un usuario válido.", nameof(usuarioId));

        if (string.IsNullOrWhiteSpace(titulo))
            throw new ArgumentException("El título del Recurso no puede estar vacío.", nameof(titulo));

        return new Recurso(Guid.CreateVersion7(), usuarioId, tipo, titulo.Trim());
    }

    public void CambiarTitulo(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new ArgumentException("El título del Recurso no puede estar vacío.", nameof(titulo));

        Titulo = titulo.Trim();
    }

    public void ActualizarUrl(string? url) => Url = url;

    public void CambiarEstado(EstadoRecurso estado) => Estado = estado;

    public void CalificarCon(RatingRecurso? rating) => Rating = rating;

    public void ActualizarNotas(string? notas) => Notas = notas;

    public void RegistrarUsoDeIA(string? herramientaIA, string? promptsUtilizados)
    {
        HerramientaIA = herramientaIA;
        PromptsUtilizados = promptsUtilizados;
    }

    public void MarcarComoEliminado() => FechaEliminacionUtc = DateTime.UtcNow;
}
