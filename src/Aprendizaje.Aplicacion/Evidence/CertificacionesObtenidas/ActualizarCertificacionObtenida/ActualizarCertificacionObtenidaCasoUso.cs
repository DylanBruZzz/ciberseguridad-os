using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ActualizarCertificacionObtenida;

public sealed class ActualizarCertificacionObtenidaCasoUso
{
    private readonly ICertificacionObtenidaRepository _certificacionesObtenidas;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarCertificacionObtenidaCasoUso(
        ICertificacionObtenidaRepository certificacionesObtenidas,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _certificacionesObtenidas = certificacionesObtenidas;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<ActualizarCertificacionObtenidaResultado> EjecutarAsync(
        ActualizarCertificacionObtenidaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.CertificacionObtenidaId == Guid.Empty)
            throw new ArgumentException("El certificacionObtenidaId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);
        if (usuario is null)
            return ActualizarCertificacionObtenidaResultado.UsuarioNoEncontrado();

        var certificacionObtenida = await _certificacionesObtenidas.ObtenerPorIdAsync(
            solicitud.CertificacionObtenidaId,
            cancellationToken);
        if (certificacionObtenida is null)
            return ActualizarCertificacionObtenidaResultado.CertificacionObtenidaNoEncontrada();

        if (certificacionObtenida.UsuarioId != solicitud.UsuarioId)
            return ActualizarCertificacionObtenidaResultado.UsuarioNoCoincide();

        certificacionObtenida.ActualizarEvidenciaUrl(solicitud.EvidenciaUrl);
        certificacionObtenida.AvanzarMadurez(solicitud.EstadoMadurez);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return ActualizarCertificacionObtenidaResultado.Actualizada();
    }
}
