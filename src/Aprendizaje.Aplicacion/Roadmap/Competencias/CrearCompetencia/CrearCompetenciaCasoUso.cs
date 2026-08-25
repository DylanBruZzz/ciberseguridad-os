using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Competencias.CrearCompetencia;

public sealed class CrearCompetenciaCasoUso
{
    private readonly ICompetenciaRepository _competencias;
    private readonly IUnitOfWork _unitOfWork;

    public CrearCompetenciaCasoUso(ICompetenciaRepository competencias, IUnitOfWork unitOfWork)
    {
        _competencias = competencias;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearCompetenciaResultado> EjecutarAsync(
        CrearCompetenciaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var competencia = Competencia.Crear(solicitud.UsuarioId, solicitud.Nombre);

        _competencias.Agregar(competencia);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new CrearCompetenciaResultado(
            competencia.Id,
            competencia.UsuarioId,
            competencia.Nombre,
            competencia.Descripcion);
    }
}
