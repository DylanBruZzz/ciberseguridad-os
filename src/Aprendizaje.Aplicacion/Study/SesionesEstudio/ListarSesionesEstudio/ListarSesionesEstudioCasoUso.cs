using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.ListarSesionesEstudio;

public sealed class ListarSesionesEstudioCasoUso
{
    private readonly ISesionEstudioRepository _sesiones;

    public ListarSesionesEstudioCasoUso(ISesionEstudioRepository sesiones)
    {
        _sesiones = sesiones;
    }

    public async Task<ListarSesionesEstudioResultado> EjecutarAsync(
        ListarSesionesEstudioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var sesiones = await _sesiones.ListarPorUsuarioAsync(solicitud.UsuarioId, cancellationToken);

        return new ListarSesionesEstudioResultado(sesiones.Select(SesionEstudioResumen.Desde).ToList());
    }
}
