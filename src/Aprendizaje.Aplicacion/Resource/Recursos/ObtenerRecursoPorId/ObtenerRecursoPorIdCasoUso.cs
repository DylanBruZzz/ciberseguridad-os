using Aprendizaje.Aplicacion.Resource.Recursos;

namespace Aprendizaje.Aplicacion.Resource.Recursos.ObtenerRecursoPorId;

public sealed class ObtenerRecursoPorIdCasoUso
{
    private readonly IConsultaRecursosV1 _consulta;

    public ObtenerRecursoPorIdCasoUso(IConsultaRecursosV1 consulta)
    {
        _consulta = consulta;
    }

    public async Task<ObtenerRecursoPorIdResultado> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var recurso = await _consulta.ObtenerDetalleAsync(id, cancellationToken);

        if (recurso is null)
            return ObtenerRecursoPorIdResultado.NoEncontrado();

        return ObtenerRecursoPorIdResultado.EncontradoCon(recurso);
    }
}
