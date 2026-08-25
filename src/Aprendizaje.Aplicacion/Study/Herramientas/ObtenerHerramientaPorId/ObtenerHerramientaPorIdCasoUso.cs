using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Study.Herramientas.ObtenerHerramientaPorId;

public sealed class ObtenerHerramientaPorIdCasoUso
{
    private readonly IHerramientaRepository _herramientas;

    public ObtenerHerramientaPorIdCasoUso(IHerramientaRepository herramientas)
    {
        _herramientas = herramientas;
    }

    public async Task<ObtenerHerramientaPorIdResultado> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El id debe ser un Guid válido.", nameof(id));

        var herramienta = await _herramientas.ObtenerPorIdAsync(id, cancellationToken);

        return herramienta is null
            ? ObtenerHerramientaPorIdResultado.NoEncontrada()
            : ObtenerHerramientaPorIdResultado.EncontradaCon(HerramientaDetalle.DesdeDominio(herramienta));
    }
}
