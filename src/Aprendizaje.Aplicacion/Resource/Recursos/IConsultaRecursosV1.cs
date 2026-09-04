using Aprendizaje.Aplicacion.Resource.Recursos.ListarRecursos;
using Aprendizaje.Aplicacion.Resource.Recursos.ObtenerRecursoPorId;

namespace Aprendizaje.Aplicacion.Resource.Recursos;

public interface IConsultaRecursosV1
{
    Task<IReadOnlyCollection<RecursoResumen>> ListarAsync(
        ListarRecursosSolicitud solicitud,
        CancellationToken cancellationToken = default);

    Task<RecursoDetalle?> ObtenerDetalleAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
