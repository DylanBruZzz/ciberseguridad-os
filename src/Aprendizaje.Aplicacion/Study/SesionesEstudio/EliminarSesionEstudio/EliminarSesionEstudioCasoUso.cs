using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Nucleo.Repositorios;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.EliminarSesionEstudio;

public sealed class EliminarSesionEstudioCasoUso
{
    private readonly ISesionEstudioRepository _sesiones;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarSesionEstudioCasoUso(
        ISesionEstudioRepository sesiones,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _sesiones = sesiones;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<EliminarSesionEstudioResultado> EjecutarAsync(
        EliminarSesionEstudioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.SesionId == Guid.Empty)
            throw new ArgumentException("El sesionId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);

        if (usuario is null)
            return EliminarSesionEstudioResultado.UsuarioNoEncontrado();

        var sesion = await _sesiones.ObtenerPorIdAsync(solicitud.SesionId, cancellationToken);

        if (sesion is null)
            return EliminarSesionEstudioResultado.SesionNoEncontrada();

        if (sesion.UsuarioId != solicitud.UsuarioId)
            return EliminarSesionEstudioResultado.UsuarioNoCoincide();

        sesion.MarcarComoEliminado();

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return EliminarSesionEstudioResultado.Eliminada();
    }
}
