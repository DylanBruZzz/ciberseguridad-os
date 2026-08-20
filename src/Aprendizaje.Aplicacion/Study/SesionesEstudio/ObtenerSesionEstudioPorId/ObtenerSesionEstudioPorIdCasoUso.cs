using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.ObtenerSesionEstudioPorId;

public sealed class ObtenerSesionEstudioPorIdCasoUso
{
    private readonly ISesionEstudioRepository _sesiones;

    public ObtenerSesionEstudioPorIdCasoUso(ISesionEstudioRepository sesiones)
    {
        _sesiones = sesiones;
    }

    public async Task<ObtenerSesionEstudioPorIdResultado> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var sesion = await _sesiones.ObtenerPorIdAsync(id, cancellationToken);

        return sesion is null
            ? ObtenerSesionEstudioPorIdResultado.NoEncontrada()
            : ObtenerSesionEstudioPorIdResultado.Encontrada(SesionEstudioDetalle.Desde(sesion));
    }
}
