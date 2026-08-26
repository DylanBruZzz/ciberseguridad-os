namespace Aprendizaje.Aplicacion.Analytics.Temas;

public interface IConsultaResumenTema
{
    Task<ResumenTemaDto?> ObtenerAsync(
        Guid usuarioId,
        Guid temaId,
        CancellationToken cancellationToken = default);
}
