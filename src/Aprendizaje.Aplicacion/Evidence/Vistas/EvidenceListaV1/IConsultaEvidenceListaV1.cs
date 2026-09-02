namespace Aprendizaje.Aplicacion.Evidence.Vistas.EvidenceListaV1;

public interface IConsultaEvidenceListaV1
{
    Task<EvidenceListaV1Dto> ObtenerAsync(
        ObtenerEvidenceListaV1Solicitud solicitud,
        CancellationToken cancellationToken = default);
}
