using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ObtenerCertificacionObtenidaPorId;

public sealed class ObtenerCertificacionObtenidaPorIdCasoUso
{
    private readonly ICertificacionObtenidaRepository _certificacionesObtenidas;

    public ObtenerCertificacionObtenidaPorIdCasoUso(ICertificacionObtenidaRepository certificacionesObtenidas)
    {
        _certificacionesObtenidas = certificacionesObtenidas;
    }

    public async Task<ObtenerCertificacionObtenidaPorIdResultado> EjecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El id debe ser un Guid válido.", nameof(id));

        var certificacionObtenida = await _certificacionesObtenidas.ObtenerPorIdAsync(id, cancellationToken);

        if (certificacionObtenida is null)
            return ObtenerCertificacionObtenidaPorIdResultado.NoEncontrada();

        return ObtenerCertificacionObtenidaPorIdResultado.EncontradaCon(new CertificacionObtenidaDetalle(
            certificacionObtenida.Id,
            certificacionObtenida.UsuarioId,
            certificacionObtenida.CertificacionId,
            certificacionObtenida.FechaObtencion,
            certificacionObtenida.EvidenciaUrl,
            certificacionObtenida.EstadoMadurez));
    }
}
