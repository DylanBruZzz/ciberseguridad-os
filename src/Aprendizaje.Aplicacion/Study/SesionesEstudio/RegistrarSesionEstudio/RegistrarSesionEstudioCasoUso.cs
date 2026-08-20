using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.RegistrarSesionEstudio;

public sealed class RegistrarSesionEstudioCasoUso
{
    private readonly ISesionEstudioRepository _sesiones;
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarSesionEstudioCasoUso(
        ISesionEstudioRepository sesiones,
        ITemaRepository temas,
        IUnitOfWork unitOfWork)
    {
        _sesiones = sesiones;
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegistrarSesionEstudioResultado> EjecutarAsync(
        RegistrarSesionEstudioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return RegistrarSesionEstudioResultado.TemaNoEncontrado();

        if (tema.UsuarioId != solicitud.UsuarioId)
            return RegistrarSesionEstudioResultado.UsuarioNoCoincide();

        var sesion = SesionEstudio.Registrar(
            solicitud.UsuarioId,
            solicitud.TemaId,
            solicitud.Fecha,
            solicitud.DuracionMinutos,
            solicitud.Tipo,
            solicitud.Notas);

        _sesiones.Agregar(sesion);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return RegistrarSesionEstudioResultado.Creada(sesion);
    }
}
