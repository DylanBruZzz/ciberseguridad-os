using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ActualizarArtefactoTecnico;

public sealed class ActualizarArtefactoTecnicoCasoUso
{
    private readonly IArtefactoTecnicoRepository _artefactosTecnicos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarArtefactoTecnicoCasoUso(
        IArtefactoTecnicoRepository artefactosTecnicos,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _artefactosTecnicos = artefactosTecnicos;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<ActualizarArtefactoTecnicoResultado> EjecutarAsync(
        ActualizarArtefactoTecnicoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.ArtefactoTecnicoId == Guid.Empty)
            throw new ArgumentException("El artefactoTecnicoId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);
        if (usuario is null)
            return ActualizarArtefactoTecnicoResultado.UsuarioNoEncontrado();

        var artefacto = await _artefactosTecnicos.ObtenerPorIdAsync(solicitud.ArtefactoTecnicoId, cancellationToken);
        if (artefacto is null)
            return ActualizarArtefactoTecnicoResultado.ArtefactoTecnicoNoEncontrado();

        if (artefacto.UsuarioId != solicitud.UsuarioId)
            return ActualizarArtefactoTecnicoResultado.UsuarioNoCoincide();

        artefacto.CambiarTipoArtefacto(solicitud.TipoArtefacto);
        artefacto.CambiarNombre(solicitud.Nombre);
        artefacto.ActualizarContenidoOUrl(solicitud.ContenidoOUrl);
        artefacto.ActualizarLenguajeTecnologia(solicitud.LenguajeTecnologia);
        artefacto.AvanzarMadurez(solicitud.EstadoMadurez);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return ActualizarArtefactoTecnicoResultado.Actualizado();
    }
}
