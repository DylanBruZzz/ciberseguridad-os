using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Study.Herramientas.ListarHerramientas;

public sealed class ListarHerramientasCasoUso
{
    private readonly IHerramientaRepository _herramientas;

    public ListarHerramientasCasoUso(IHerramientaRepository herramientas)
    {
        _herramientas = herramientas;
    }

    public async Task<ListarHerramientasResultado> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        var herramientas = await _herramientas.ListarAsync(cancellationToken);

        return new ListarHerramientasResultado(
            herramientas.Select(HerramientaResumen.DesdeDominio).ToArray());
    }
}
