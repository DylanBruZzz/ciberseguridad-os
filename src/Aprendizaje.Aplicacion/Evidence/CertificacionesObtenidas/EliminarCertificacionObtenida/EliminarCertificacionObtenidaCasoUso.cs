using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.EliminarCertificacionObtenida;

public sealed class EliminarCertificacionObtenidaCasoUso
{
    private readonly ICertificacionObtenidaRepository _certificacionesObtenidas;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarCertificacionObtenidaCasoUso(
        ICertificacionObtenidaRepository certificacionesObtenidas,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _certificacionesObtenidas = certificacionesObtenidas;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<EliminarCertificacionObtenidaResultado> EjecutarAsync(
        EliminarCertificacionObtenidaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.CertificacionObtenidaId == Guid.Empty)
            throw new ArgumentException("El certificacionObtenidaId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);
        if (usuario is null)
            return EliminarCertificacionObtenidaResultado.UsuarioNoEncontrado();

        var certificacionObtenida = await _certificacionesObtenidas.ObtenerPorIdAsync(
            solicitud.CertificacionObtenidaId,
            cancellationToken);
        if (certificacionObtenida is null)
            return EliminarCertificacionObtenidaResultado.CertificacionObtenidaNoEncontrada();

        if (certificacionObtenida.UsuarioId != solicitud.UsuarioId)
            return EliminarCertificacionObtenidaResultado.UsuarioNoCoincide();

        certificacionObtenida.MarcarComoEliminado();

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return EliminarCertificacionObtenidaResultado.Eliminada();
    }
}
