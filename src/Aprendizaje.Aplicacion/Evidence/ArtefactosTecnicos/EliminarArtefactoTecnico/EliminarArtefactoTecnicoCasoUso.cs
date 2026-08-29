using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.EliminarArtefactoTecnico;

public sealed class EliminarArtefactoTecnicoCasoUso
{
    private readonly IArtefactoTecnicoRepository _artefactosTecnicos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarArtefactoTecnicoCasoUso(
        IArtefactoTecnicoRepository artefactosTecnicos,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _artefactosTecnicos = artefactosTecnicos;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<EliminarArtefactoTecnicoResultado> EjecutarAsync(
        EliminarArtefactoTecnicoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.ArtefactoTecnicoId == Guid.Empty)
            throw new ArgumentException("El artefactoTecnicoId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);
        if (usuario is null)
            return EliminarArtefactoTecnicoResultado.UsuarioNoEncontrado();

        var artefacto = await _artefactosTecnicos.ObtenerPorIdAsync(solicitud.ArtefactoTecnicoId, cancellationToken);
        if (artefacto is null)
            return EliminarArtefactoTecnicoResultado.ArtefactoTecnicoNoEncontrado();

        if (artefacto.UsuarioId != solicitud.UsuarioId)
            return EliminarArtefactoTecnicoResultado.UsuarioNoCoincide();

        artefacto.MarcarComoEliminado();

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return EliminarArtefactoTecnicoResultado.Eliminado();
    }
}
