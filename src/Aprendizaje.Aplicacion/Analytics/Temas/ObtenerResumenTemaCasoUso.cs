namespace Aprendizaje.Aplicacion.Analytics.Temas;

public sealed class ObtenerResumenTemaCasoUso
{
    private readonly IConsultaResumenTema _consulta;

    public ObtenerResumenTemaCasoUso(IConsultaResumenTema consulta)
    {
        _consulta = consulta;
    }

    public async Task<ObtenerResumenTemaResultado> EjecutarAsync(
        ObtenerResumenTemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var resumen = await _consulta.ObtenerAsync(solicitud.UsuarioId, solicitud.TemaId, cancellationToken);

        return resumen is null
            ? ObtenerResumenTemaResultado.NoEncontrado()
            : ObtenerResumenTemaResultado.EncontradoCon(resumen);
    }
}
