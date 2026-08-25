using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.CrearCertificacionObtenida;

public sealed class CrearCertificacionObtenidaCasoUso
{
    private readonly ICertificacionObtenidaRepository _certificacionesObtenidas;
    private readonly ICertificacionRepository _certificaciones;
    private readonly IUnitOfWork _unitOfWork;

    public CrearCertificacionObtenidaCasoUso(
        ICertificacionObtenidaRepository certificacionesObtenidas,
        ICertificacionRepository certificaciones,
        IUnitOfWork unitOfWork)
    {
        _certificacionesObtenidas = certificacionesObtenidas;
        _certificaciones = certificaciones;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearCertificacionObtenidaResultado> EjecutarAsync(
        CrearCertificacionObtenidaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.CertificacionId == Guid.Empty)
            throw new ArgumentException("El certificacionId debe ser un Guid válido.", nameof(solicitud));

        var certificacion = await _certificaciones.ObtenerPorIdAsync(solicitud.CertificacionId, cancellationToken);

        if (certificacion is null)
            return CrearCertificacionObtenidaResultado.CertificacionNoEncontrada();

        var certificacionObtenida = CertificacionObtenida.Registrar(
            solicitud.UsuarioId,
            solicitud.CertificacionId,
            solicitud.FechaObtencion);

        _certificacionesObtenidas.Agregar(certificacionObtenida);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return CrearCertificacionObtenidaResultado.Creada(certificacionObtenida);
    }
}
