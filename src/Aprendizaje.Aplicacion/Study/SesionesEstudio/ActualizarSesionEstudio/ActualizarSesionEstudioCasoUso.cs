using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Nucleo.Repositorios;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.ActualizarSesionEstudio;

public sealed class ActualizarSesionEstudioCasoUso
{
    private readonly ISesionEstudioRepository _sesiones;
    private readonly ITemaRepository _temas;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarSesionEstudioCasoUso(
        ISesionEstudioRepository sesiones,
        ITemaRepository temas,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _sesiones = sesiones;
        _temas = temas;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<ActualizarSesionEstudioResultado> EjecutarAsync(
        ActualizarSesionEstudioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.SesionId == Guid.Empty)
            throw new ArgumentException("El sesionId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);

        if (usuario is null)
            return ActualizarSesionEstudioResultado.UsuarioNoEncontrado();

        var sesion = await _sesiones.ObtenerPorIdAsync(solicitud.SesionId, cancellationToken);

        if (sesion is null)
            return ActualizarSesionEstudioResultado.SesionNoEncontrada();

        if (sesion.UsuarioId != solicitud.UsuarioId)
            return ActualizarSesionEstudioResultado.UsuarioNoCoincide();

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return ActualizarSesionEstudioResultado.TemaNoEncontrado();

        if (tema.UsuarioId != solicitud.UsuarioId)
            return ActualizarSesionEstudioResultado.UsuarioNoCoincide();

        sesion.CambiarTema(solicitud.TemaId);
        sesion.CambiarFecha(solicitud.Fecha);
        sesion.CorregirDuracion(solicitud.DuracionMinutos);
        sesion.CambiarTipo(solicitud.Tipo);
        sesion.ActualizarNotas(solicitud.Notas);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return ActualizarSesionEstudioResultado.Actualizada();
    }
}
