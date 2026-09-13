using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Roadmap;

/// <summary>
/// Entidad interna del agregado Tema. No hereda de AggregateRoot: no tiene repositorio propio
/// y no se muta desde fuera del agregado — toda escritura pasa por métodos de Tema.
/// </summary>
public sealed class CriterioTema : Entidad
{
    public const int DescripcionMaxLength = 500;

    public Guid TemaId { get; private set; }
    public TipoCriterio Tipo { get; private set; }
    public string? Descripcion { get; private set; }
    public bool Cumplido { get; private set; }
    public DateTime? FechaCumplido { get; private set; }

    private CriterioTema() { } // requerido por EF Core

    internal CriterioTema(Guid temaId, TipoCriterio tipo, string descripcion) : base(Guid.CreateVersion7())
    {
        TemaId = temaId;
        Tipo = tipo;
        Descripcion = NormalizarDescripcion(descripcion);
    }

    internal void ActualizarDescripcion(string descripcion) =>
        Descripcion = NormalizarDescripcion(descripcion);

    private static string NormalizarDescripcion(string descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException("La descripción del criterio no puede estar vacía.", nameof(descripcion));

        var normalizada = descripcion.Trim();
        if (normalizada.Length > DescripcionMaxLength)
            throw new ArgumentException(
                $"La descripción del criterio no puede superar {DescripcionMaxLength} caracteres.",
                nameof(descripcion));

        return normalizada;
    }

    internal void Marcar()
    {
        Cumplido = true;
        FechaCumplido = DateTime.UtcNow;
    }

    internal void Desmarcar()
    {
        Cumplido = false;
        FechaCumplido = null;
    }
}
