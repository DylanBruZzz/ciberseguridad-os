using Aprendizaje.Dominio.Resource.Repositorios;

namespace Aprendizaje.Aplicacion.Resource.Recursos.ObtenerRecursoPorId;

public sealed class ObtenerRecursoPorIdCasoUso
{
    private readonly IRecursoRepository _recursos;

    public ObtenerRecursoPorIdCasoUso(IRecursoRepository recursos)
    {
        _recursos = recursos;
    }

    public async Task<ObtenerRecursoPorIdResultado> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var recurso = await _recursos.ObtenerPorIdAsync(id, cancellationToken);

        if (recurso is null)
            return ObtenerRecursoPorIdResultado.NoEncontrado();

        return ObtenerRecursoPorIdResultado.EncontradoCon(new RecursoDetalle(
            recurso.Id,
            recurso.UsuarioId,
            recurso.Tipo,
            recurso.Titulo,
            recurso.Url,
            recurso.Estado,
            recurso.Rating?.Valor,
            recurso.Notas,
            recurso.HerramientaIA,
            recurso.PromptsUtilizados));
    }
}
