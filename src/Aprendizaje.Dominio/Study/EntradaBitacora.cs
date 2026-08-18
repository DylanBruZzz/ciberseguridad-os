using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Study;

/// <summary>
/// Diario técnico libre, opcionalmente ligado a un Tema. Sin Domain Event: a diferencia de
/// SesionRegistradaEvento o TemaDominadoEvento, nada en el diseño ya cerrado reacciona a la
/// creación de una entrada de bitácora. Editable, no append-only como Nota — corregir una
/// entrada reciente es una operación legítima.
/// </summary>
public sealed class EntradaBitacora : AggregateRoot, IEliminableLogicamente
{
    public Guid UsuarioId { get; private set; }
    public Guid? TemaId { get; private set; }
    public DateTime Fecha { get; private set; }
    public string Texto { get; private set; } = null!;

    public DateTime? FechaEliminacionUtc { get; private set; }

    private EntradaBitacora() { } // requerido por EF Core

    private EntradaBitacora(Guid id, Guid usuarioId, Guid? temaId, string texto) : base(id)
    {
        UsuarioId = usuarioId;
        TemaId = temaId;
        Fecha = DateTime.UtcNow;
        Texto = texto;
    }

    public static EntradaBitacora Escribir(Guid usuarioId, string texto, Guid? temaId = null)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("La entrada de bitácora debe pertenecer a un usuario válido.", nameof(usuarioId));

        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("El texto de la entrada de bitácora no puede estar vacío.", nameof(texto));

        return new EntradaBitacora(Guid.CreateVersion7(), usuarioId, temaId, texto.Trim());
    }

    public void ActualizarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("El texto de la entrada de bitácora no puede estar vacío.", nameof(texto));

        Texto = texto.Trim();
    }

    public void MarcarComoEliminado() => FechaEliminacionUtc = DateTime.UtcNow;
}
