namespace Aprendizaje.Aplicacion.Analytics.Certificaciones;

public sealed class ObtenerResumenCertificacionesCasoUso
{
    private readonly IConsultaResumenCertificaciones _consulta;

    public ObtenerResumenCertificacionesCasoUso(IConsultaResumenCertificaciones consulta)
    {
        _consulta = consulta;
    }

    public Task<IReadOnlyCollection<ResumenCertificacionDto>> EjecutarAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(usuarioId));

        return _consulta.ListarAsync(usuarioId, cancellationToken);
    }
}
