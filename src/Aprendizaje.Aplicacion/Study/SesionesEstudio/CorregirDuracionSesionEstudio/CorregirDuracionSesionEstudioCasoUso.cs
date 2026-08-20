using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.CorregirDuracionSesionEstudio;

public sealed class CorregirDuracionSesionEstudioCasoUso
{
    private readonly ISesionEstudioRepository _sesiones;
    private readonly IUnitOfWork _unitOfWork;

    public CorregirDuracionSesionEstudioCasoUso(
        ISesionEstudioRepository sesiones,
        IUnitOfWork unitOfWork)
    {
        _sesiones = sesiones;
        _unitOfWork = unitOfWork;
    }

    public async Task<CorregirDuracionSesionEstudioResultado> EjecutarAsync(
        CorregirDuracionSesionEstudioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.SesionId == Guid.Empty)
            throw new ArgumentException("El sesionId debe ser un Guid válido.", nameof(solicitud));

        var sesion = await _sesiones.ObtenerPorIdAsync(solicitud.SesionId, cancellationToken);

        if (sesion is null)
            return CorregirDuracionSesionEstudioResultado.SesionNoEncontrada();

        sesion.CorregirDuracion(solicitud.DuracionMinutos);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return CorregirDuracionSesionEstudioResultado.Actualizada();
    }
}
