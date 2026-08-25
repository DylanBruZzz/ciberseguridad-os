using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Certificaciones.CrearCertificacion;

public sealed class CrearCertificacionCasoUso
{
    private readonly ICertificacionRepository _certificaciones;
    private readonly IUnitOfWork _unitOfWork;

    public CrearCertificacionCasoUso(ICertificacionRepository certificaciones, IUnitOfWork unitOfWork)
    {
        _certificaciones = certificaciones;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearCertificacionResultado> EjecutarAsync(
        CrearCertificacionSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var certificacion = Certificacion.Crear(solicitud.Nombre, solicitud.TipoCosto);

        _certificaciones.Agregar(certificacion);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new CrearCertificacionResultado(
            certificacion.Id,
            certificacion.Nombre,
            certificacion.Proveedor,
            certificacion.TipoCosto,
            certificacion.Url);
    }
}
