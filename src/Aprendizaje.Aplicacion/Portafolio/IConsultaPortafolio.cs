namespace Aprendizaje.Aplicacion.Portafolio;

public interface IConsultaPortafolio
{
    Task<PortafolioDto> ObtenerAsync(
        ObtenerPortafolioSolicitud solicitud,
        CancellationToken cancellationToken = default);
}
