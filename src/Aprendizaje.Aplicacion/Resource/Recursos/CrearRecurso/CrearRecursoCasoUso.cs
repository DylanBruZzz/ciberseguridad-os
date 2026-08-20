using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Resource.Repositorios;

namespace Aprendizaje.Aplicacion.Resource.Recursos.CrearRecurso;

public sealed class CrearRecursoCasoUso
{
    private readonly IRecursoRepository _recursos;
    private readonly IUnitOfWork _unitOfWork;

    public CrearRecursoCasoUso(IRecursoRepository recursos, IUnitOfWork unitOfWork)
    {
        _recursos = recursos;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearRecursoResultado> EjecutarAsync(
        CrearRecursoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var recurso = Recurso.Guardar(solicitud.UsuarioId, solicitud.Tipo, solicitud.Titulo);
        recurso.ActualizarUrl(solicitud.Url);

        _recursos.Agregar(recurso);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new CrearRecursoResultado(
            recurso.Id,
            recurso.UsuarioId,
            recurso.Tipo,
            recurso.Titulo,
            recurso.Url,
            recurso.Estado);
    }
}
