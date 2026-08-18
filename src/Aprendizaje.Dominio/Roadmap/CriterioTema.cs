using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Roadmap;

/// <summary>
/// Entidad interna del agregado Tema. No hereda de AggregateRoot: no tiene repositorio propio
/// y no se muta desde fuera del agregado — toda escritura pasa por métodos de Tema.
/// </summary>
public sealed class CriterioTema : Entidad
{
    public Guid TemaId { get; private set; }
    public TipoCriterio Tipo { get; private set; }
    public bool Cumplido { get; private set; }
    public DateTime? FechaCumplido { get; private set; }

    private CriterioTema() { } // requerido por EF Core

    internal CriterioTema(Guid temaId, TipoCriterio tipo) : base(Guid.CreateVersion7())
    {
        TemaId = temaId;
        Tipo = tipo;
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
