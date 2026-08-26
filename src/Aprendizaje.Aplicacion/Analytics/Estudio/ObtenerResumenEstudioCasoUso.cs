namespace Aprendizaje.Aplicacion.Analytics.Estudio;

public sealed class ObtenerResumenEstudioCasoUso
{
    private readonly IConsultaResumenEstudio _consulta;

    public ObtenerResumenEstudioCasoUso(IConsultaResumenEstudio consulta)
    {
        _consulta = consulta;
    }

    public Task<ResumenEstudioDto> EjecutarAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(usuarioId));

        return _consulta.ObtenerAsync(usuarioId, cancellationToken);
    }
}
