using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.VincularHerramientaASesionEstudio;

public sealed class VincularHerramientaASesionEstudioCasoUso
{
    private readonly ISesionEstudioRepository _sesiones;
    private readonly IHerramientaRepository _herramientas;
    private readonly IUnitOfWork _unitOfWork;

    public VincularHerramientaASesionEstudioCasoUso(
        ISesionEstudioRepository sesiones,
        IHerramientaRepository herramientas,
        IUnitOfWork unitOfWork)
    {
        _sesiones = sesiones;
        _herramientas = herramientas;
        _unitOfWork = unitOfWork;
    }

    public async Task<VincularHerramientaASesionEstudioResultado> EjecutarAsync(
        VincularHerramientaASesionEstudioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.SesionId == Guid.Empty)
            throw new ArgumentException("El sesionId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.HerramientaId == Guid.Empty)
            throw new ArgumentException("El herramientaId debe ser un Guid válido.", nameof(solicitud));

        var sesion = await _sesiones.ObtenerPorIdAsync(solicitud.SesionId, cancellationToken);

        if (sesion is null)
            return VincularHerramientaASesionEstudioResultado.SesionNoEncontrada();

        var herramienta = await _herramientas.ObtenerPorIdAsync(solicitud.HerramientaId, cancellationToken);

        if (herramienta is null)
            return VincularHerramientaASesionEstudioResultado.HerramientaNoEncontrada();

        var yaExiste = await _sesiones.ExisteVinculoHerramientaAsync(
            solicitud.SesionId,
            solicitud.HerramientaId,
            cancellationToken);

        if (!yaExiste)
        {
            _sesiones.VincularHerramienta(solicitud.SesionId, solicitud.HerramientaId);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);
        }

        return VincularHerramientaASesionEstudioResultado.Actualizado();
    }
}
