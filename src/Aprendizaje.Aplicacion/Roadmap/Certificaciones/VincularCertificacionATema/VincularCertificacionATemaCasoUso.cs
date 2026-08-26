using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Certificaciones.VincularCertificacionATema;

public sealed class VincularCertificacionATemaCasoUso
{
    private readonly ICertificacionRepository _certificaciones;
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public VincularCertificacionATemaCasoUso(
        ICertificacionRepository certificaciones,
        ITemaRepository temas,
        IUnitOfWork unitOfWork)
    {
        _certificaciones = certificaciones;
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<VincularCertificacionATemaResultado> EjecutarAsync(
        VincularCertificacionATemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.CertificacionId == Guid.Empty)
            throw new ArgumentException("El certificacionId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var certificacion = await _certificaciones.ObtenerPorIdAsync(solicitud.CertificacionId, cancellationToken);

        if (certificacion is null)
            return VincularCertificacionATemaResultado.CertificacionNoEncontrada();

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return VincularCertificacionATemaResultado.TemaNoEncontrado();

        var yaExiste = await _certificaciones.ExisteVinculoTemaAsync(
            solicitud.CertificacionId,
            solicitud.TemaId,
            cancellationToken);

        if (!yaExiste)
        {
            _certificaciones.VincularTema(solicitud.CertificacionId, solicitud.TemaId);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);
        }

        return VincularCertificacionATemaResultado.Actualizado();
    }
}
