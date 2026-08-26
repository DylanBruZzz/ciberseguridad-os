namespace Aprendizaje.Aplicacion.Analytics.Certificaciones;

public interface IConsultaResumenCertificaciones
{
    Task<IReadOnlyCollection<ResumenCertificacionDto>> ListarAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);
}
