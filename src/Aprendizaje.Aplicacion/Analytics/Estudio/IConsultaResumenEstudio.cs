namespace Aprendizaje.Aplicacion.Analytics.Estudio;

public interface IConsultaResumenEstudio
{
    Task<ResumenEstudioDto> ObtenerAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
