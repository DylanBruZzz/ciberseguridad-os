using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.CrearArtefactoTecnico;

public sealed class CrearArtefactoTecnicoCasoUso
{
    private readonly IArtefactoTecnicoRepository _artefactosTecnicos;
    private readonly IUnitOfWork _unitOfWork;

    public CrearArtefactoTecnicoCasoUso(
        IArtefactoTecnicoRepository artefactosTecnicos,
        IUnitOfWork unitOfWork)
    {
        _artefactosTecnicos = artefactosTecnicos;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearArtefactoTecnicoResultado> EjecutarAsync(
        CrearArtefactoTecnicoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var artefactoTecnico = ArtefactoTecnico.Crear(
            solicitud.UsuarioId,
            solicitud.TipoArtefacto,
            solicitud.Nombre);

        _artefactosTecnicos.Agregar(artefactoTecnico);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new CrearArtefactoTecnicoResultado(
            artefactoTecnico.Id,
            artefactoTecnico.UsuarioId,
            artefactoTecnico.TipoArtefacto,
            artefactoTecnico.Nombre,
            artefactoTecnico.ContenidoOUrl,
            artefactoTecnico.LenguajeTecnologia,
            artefactoTecnico.EstadoMadurez);
    }
}
